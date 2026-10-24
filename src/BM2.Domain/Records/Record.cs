using BM2.Contracts.Categories;
using BM2.Contracts.Records;
using BM2.Domain.Accounts;
using BM2.Domain.Categories;
using BM2.Domain.Common;

namespace BM2.Domain.Records;

/// <summary>An expense, income or transfer. Amounts are never negative; the type gives the direction.</summary>
public sealed class Record
{
    private readonly List<RecordTag> _tags = [];

    private Record()
    {
        Currency = "";
    }

    public Guid Id { get; private set; }

    /// <summary>The account the money leaves (expense, transfer) or enters (income).</summary>
    public Guid AccountId { get; private set; }

    public RecordType Type { get; private set; }

    public RecordStatus Status { get; private set; }

    public DateOnly OccurredOn { get; private set; }

    public decimal Amount { get; private set; }

    /// <summary>Transaction currency of <see cref="Amount"/>.</summary>
    public string Currency { get; private set; }

    /// <summary>The amount in the account's currency.</summary>
    public decimal AccountAmount { get; private set; }

    public Guid? CategoryId { get; private set; }

    public Guid? TransferAccountId { get; private set; }

    /// <summary>The amount received, in the target account's currency.</summary>
    public decimal? TransferAmount { get; private set; }

    public string? Name { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public Account Account { get; private set; } = null!;

    public Account? TransferAccount { get; private set; }

    public Category? Category { get; private set; }

    public IReadOnlyCollection<RecordTag> Tags => _tags;

    public static Result<Record> Create(RecordDraft draft, RecordContext context, Guid userId, DateTimeOffset now)
    {
        var record = new Record { CreatedByUserId = userId, CreatedAt = now };
        var error = record.Check(draft, context, isNew: true);
        if (error is not null)
        {
            return error;
        }

        record.Apply(draft, context);
        return record;
    }

    public Result Update(RecordDraft draft, RecordContext context, DateTimeOffset now)
    {
        var error = Check(draft, context, isNew: false);
        if (error is not null)
        {
            return error;
        }

        Apply(draft, context);
        UpdatedAt = now;
        return Result.Success;
    }

    // Archived accounts and categories only block new assignments, so old records stay editable.
    private Error? Check(RecordDraft draft, RecordContext context, bool isNew)
    {
        var account = context.Account;
        if (account is null || account.WalletId != context.WalletId)
        {
            return Error.NotFound();
        }

        if (account.IsArchived && (isNew || account.Id != AccountId))
        {
            return Error.Validation(nameof(AccountId), "This account is archived.");
        }

        var typeError = draft.Type == RecordType.Transfer
            ? CheckTransfer(draft, context, account, isNew)
            : CheckCategory(draft, context, isNew);
        if (typeError is not null)
        {
            return typeError;
        }

        var walletTags = context.Tags.Where(t => t.WalletId == context.WalletId).Select(t => t.Id).ToHashSet();
        return draft.TagIds.All(walletTags.Contains) ? null : Error.NotFound();
    }

    private Error? CheckTransfer(RecordDraft draft, RecordContext context, AccountRef account, bool isNew)
    {
        if (draft.TransferAccountId is null)
        {
            return Error.Validation(nameof(TransferAccountId), "Choose the account the money goes to.");
        }

        var target = context.TransferAccount;
        if (target is null || target.WalletId != context.WalletId)
        {
            return Error.NotFound();
        }

        if (target.Id == account.Id)
        {
            return Error.Validation(nameof(TransferAccountId), "Choose a different account.");
        }

        if (target.IsArchived && (isNew || target.Id != TransferAccountId))
        {
            return Error.Validation(nameof(TransferAccountId), "This account is archived.");
        }

        if (draft.Currency != account.Currency)
        {
            return Error.Validation(nameof(Currency), "A transfer uses the currency of the account it leaves.");
        }

        if (draft.CategoryId is not null)
        {
            return Error.Validation(nameof(CategoryId), "Transfers have no category.");
        }

        return target.Currency != account.Currency && draft.TransferAmount is null
            ? Error.Validation(nameof(TransferAmount), "Enter the amount received.")
            : null;
    }

    private Error? CheckCategory(RecordDraft draft, RecordContext context, bool isNew)
    {
        if (draft.TransferAccountId is not null)
        {
            return Error.Validation(nameof(TransferAccountId), "Only transfers go to another account.");
        }

        if (draft.CategoryId is null)
        {
            return Error.Validation(nameof(CategoryId), "Choose a category.");
        }

        var category = context.Category;
        if (category is null || category.WalletId != context.WalletId)
        {
            return Error.NotFound();
        }

        var expected = draft.Type == RecordType.Income ? CategoryType.Income : CategoryType.Expense;
        if (category.Type != expected)
        {
            return Error.Validation(nameof(CategoryId), $"Choose an {expected.ToString().ToLowerInvariant()} category.");
        }

        return category.IsArchived && (isNew || category.Id != CategoryId)
            ? Error.Validation(nameof(CategoryId), "This category is archived.")
            : null;
    }

    private void Apply(RecordDraft draft, RecordContext context)
    {
        var account = context.Account!;
        Type = draft.Type;
        Status = draft.Status;
        OccurredOn = draft.OccurredOn;
        AccountId = account.Id;
        Amount = draft.Amount;
        Currency = draft.Currency;
        AccountAmount = draft.Currency == account.Currency ? draft.Amount : draft.AccountAmount;
        Name = string.IsNullOrWhiteSpace(draft.Name) ? null : draft.Name.Trim();

        if (draft.Type == RecordType.Transfer)
        {
            var target = context.TransferAccount!;
            CategoryId = null;
            TransferAccountId = target.Id;
            TransferAmount = target.Currency == account.Currency ? draft.Amount : draft.TransferAmount;
        }
        else
        {
            CategoryId = draft.CategoryId;
            TransferAccountId = null;
            TransferAmount = null;
        }

        SetTags(draft.TagIds);
    }

    // Diff instead of clear-and-add, so EF never tracks two join rows with the same key.
    private void SetTags(IReadOnlyList<Guid> tagIds)
    {
        var wanted = tagIds.ToHashSet();
        _tags.RemoveAll(t => !wanted.Contains(t.TagId));
        foreach (var tagId in wanted.Where(id => _tags.TrueForAll(t => t.TagId != id)))
        {
            _tags.Add(new RecordTag(tagId));
        }
    }
}
