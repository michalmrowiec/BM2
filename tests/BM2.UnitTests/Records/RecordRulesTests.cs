using BM2.Contracts.Categories;
using BM2.Contracts.Records;
using BM2.Domain.Common;
using BM2.Domain.Records;
using Record = BM2.Domain.Records.Record;

namespace BM2.UnitTests.Records;

public class RecordRulesTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 24, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid W = Guid.NewGuid(), User = Guid.NewGuid();

    private static readonly AccountRef Pln = new(Guid.NewGuid(), W, "PLN", false);
    private static readonly AccountRef Pln2 = new(Guid.NewGuid(), W, "PLN", false);
    private static readonly AccountRef Eur = new(Guid.NewGuid(), W, "EUR", false);
    private static readonly AccountRef Archived = new(Guid.NewGuid(), W, "PLN", true);
    private static readonly AccountRef ForeignAccount = new(Guid.NewGuid(), Guid.NewGuid(), "PLN", false);
    private static readonly CategoryRef Groceries = new(Guid.NewGuid(), W, CategoryType.Expense, false);
    private static readonly CategoryRef Salary = new(Guid.NewGuid(), W, CategoryType.Income, false);
    private static readonly CategoryRef OldHobby = new(Guid.NewGuid(), W, CategoryType.Expense, true);
    private static readonly CategoryRef ForeignCategory = new(Guid.NewGuid(), Guid.NewGuid(), CategoryType.Expense, false);
    private static readonly TagRef House = new(Guid.NewGuid(), W);
    private static readonly TagRef ForeignTag = new(Guid.NewGuid(), Guid.NewGuid());

    private static readonly AccountRef[] Accounts = [Pln, Pln2, Eur, Archived, ForeignAccount];
    private static readonly CategoryRef[] Categories = [Groceries, Salary, OldHobby, ForeignCategory];
    private static readonly TagRef[] Tags = [House, ForeignTag];

    private static RecordDraft Expense() => new(
        RecordType.Expense, RecordStatus.Completed, new DateOnly(2026, 10, 24), Pln.Id, 12.5m, "PLN", 12.5m,
        Groceries.Id, null, null, null, []);

    private static RecordDraft Income() => Expense() with { Type = RecordType.Income, CategoryId = Salary.Id };

    private static RecordDraft Transfer(AccountRef from, AccountRef to) => Expense() with
    {
        Type = RecordType.Transfer, AccountId = from.Id, Currency = from.Currency, CategoryId = null,
        TransferAccountId = to.Id, TransferAmount = 3m,
    };

    // Mirrors the server loader: references are looked up by id, wherever they belong.
    private static RecordContext Context(RecordDraft draft, Func<AccountRef, AccountRef>? account = null, Func<CategoryRef, CategoryRef>? category = null)
    {
        AccountRef? Find(Guid? id) => Accounts.SingleOrDefault(a => a.Id == id) is { } found ? (account ?? (a => a))(found) : null;
        var foundCategory = Categories.SingleOrDefault(c => c.Id == draft.CategoryId);
        return new RecordContext(
            W,
            Find(draft.AccountId),
            Find(draft.TransferAccountId),
            foundCategory is null ? null : (category ?? (c => c))(foundCategory),
            Tags.Where(t => draft.TagIds.Contains(t.Id)).ToList());
    }

    private static Result<Record> Create(RecordDraft draft) => Record.Create(draft, Context(draft), User, Now);

    [Fact]
    public void Valid_expense_is_created_with_author_and_time()
    {
        var record = Create(Expense()).Value;

        record.CreatedByUserId.ShouldBe(User);
        record.CreatedAt.ShouldBe(Now);
        record.UpdatedAt.ShouldBeNull();
    }

    [Fact]
    public void Expense_requires_category() => Create(Expense() with { CategoryId = null }).Error!.Field.ShouldBe("CategoryId");

    [Fact]
    public void Category_type_must_match_record_type() =>
        Create(Income() with { CategoryId = Groceries.Id }).Error!.Field.ShouldBe("CategoryId");

    [Fact]
    public void Category_from_other_wallet_is_not_found() =>
        Create(Expense() with { CategoryId = ForeignCategory.Id }).Error!.Kind.ShouldBe(ErrorKind.NotFound);

    [Fact]
    public void Unknown_category_is_not_found() =>
        Create(Expense() with { CategoryId = Guid.NewGuid() }).Error!.Kind.ShouldBe(ErrorKind.NotFound);

    [Fact]
    public void Account_from_other_wallet_is_not_found() =>
        Create(Expense() with { AccountId = ForeignAccount.Id }).Error!.Kind.ShouldBe(ErrorKind.NotFound);

    [Fact]
    public void Same_currency_normalizes_account_amount() =>
        Create(Expense() with { Amount = 10m, AccountAmount = 99m }).Value.AccountAmount.ShouldBe(10m);

    [Fact]
    public void Foreign_currency_keeps_account_amount() =>
        Create(Expense() with { Currency = "EUR", Amount = 10m, AccountAmount = 43.1m }).Value.AccountAmount.ShouldBe(43.1m);

    [Fact]
    public void Transfer_to_same_account_is_invalid() => Create(Transfer(Pln, Pln)).Error!.Field.ShouldBe("TransferAccountId");

    [Fact]
    public void Transfer_requires_target_account() =>
        Create(Transfer(Pln, Eur) with { TransferAccountId = null }).Error!.Field.ShouldBe("TransferAccountId");

    [Fact]
    public void Transfer_must_use_source_account_currency() =>
        Create(Transfer(Pln, Eur) with { Currency = "EUR" }).Error!.Field.ShouldBe("Currency");

    [Fact]
    public void Transfer_between_currencies_requires_target_amount() =>
        Create(Transfer(Pln, Eur) with { TransferAmount = null }).Error!.Field.ShouldBe("TransferAmount");

    [Fact]
    public void Transfer_between_currencies_keeps_target_amount()
    {
        var record = Create(Transfer(Pln, Eur) with { Amount = 430m, AccountAmount = 1m, TransferAmount = 100m }).Value;

        record.AccountAmount.ShouldBe(430m);
        record.TransferAmount.ShouldBe(100m);
        record.CategoryId.ShouldBeNull();
    }

    [Fact]
    public void Transfer_with_same_currency_normalizes_target_amount() =>
        Create(Transfer(Pln, Pln2) with { Amount = 50m, TransferAmount = 1m }).Value.TransferAmount.ShouldBe(50m);

    [Fact]
    public void Transfer_cannot_have_category() =>
        Create(Transfer(Pln, Eur) with { CategoryId = Groceries.Id }).Error!.Field.ShouldBe("CategoryId");

    [Fact]
    public void Expense_cannot_have_target_account() =>
        Create(Expense() with { TransferAccountId = Eur.Id }).Error!.Field.ShouldBe("TransferAccountId");

    [Fact]
    public void New_record_on_archived_account_is_invalid() =>
        Create(Expense() with { AccountId = Archived.Id }).Error!.Field.ShouldBe("AccountId");

    [Fact]
    public void Editing_record_on_archived_account_without_moving_it_succeeds()
    {
        var draft = Expense() with { AccountId = Archived.Id };
        var record = Record.Create(draft, Context(draft, account: a => a with { IsArchived = false }), User, Now).Value;

        var result = record.Update(draft with { Amount = 5m }, Context(draft), Now);

        result.IsSuccess.ShouldBeTrue();
        record.Amount.ShouldBe(5m);
    }

    [Fact]
    public void Moving_record_to_archived_account_is_invalid()
    {
        var record = Create(Expense()).Value;
        var moved = Expense() with { AccountId = Archived.Id };

        record.Update(moved, Context(moved), Now).Error!.Field.ShouldBe("AccountId");
    }

    [Fact]
    public void Editing_record_keeps_archived_category()
    {
        var draft = Expense() with { CategoryId = OldHobby.Id };
        var record = Record.Create(draft, Context(draft, category: c => c with { IsArchived = false }), User, Now).Value;

        record.Update(draft with { Amount = 5m }, Context(draft), Now).IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void Switching_to_archived_category_is_invalid()
    {
        var record = Create(Expense()).Value;
        var switched = Expense() with { CategoryId = OldHobby.Id };

        record.Update(switched, Context(switched), Now).Error!.Field.ShouldBe("CategoryId");
    }

    [Fact]
    public void Tags_from_other_wallet_are_not_found() =>
        Create(Expense() with { TagIds = [ForeignTag.Id] }).Error!.Kind.ShouldBe(ErrorKind.NotFound);

    [Fact]
    public void Unknown_tags_are_not_found() =>
        Create(Expense() with { TagIds = [Guid.NewGuid()] }).Error!.Kind.ShouldBe(ErrorKind.NotFound);

    [Fact]
    public void Update_replaces_tags()
    {
        var record = Create(Expense() with { TagIds = [House.Id] }).Value;
        var untagged = Expense() with { TagIds = [] };

        record.Update(untagged, Context(untagged), Now).IsSuccess.ShouldBeTrue();

        record.Tags.ShouldBeEmpty();
        record.UpdatedAt.ShouldBe(Now);
    }

    [Fact]
    public void Failed_update_leaves_record_unchanged()
    {
        var record = Create(Expense()).Value;
        var invalid = Expense() with { Amount = 99m, CategoryId = Salary.Id };

        record.Update(invalid, Context(invalid), Now).IsSuccess.ShouldBeFalse();

        record.Amount.ShouldBe(12.5m);
        record.CategoryId.ShouldBe(Groceries.Id);
    }

    [Fact]
    public void Blank_name_becomes_null() => Create(Expense() with { Name = "  " }).Value.Name.ShouldBeNull();

    [Fact]
    public void Name_is_trimmed() => Create(Expense() with { Name = " Bakery " }).Value.Name.ShouldBe("Bakery");
}
