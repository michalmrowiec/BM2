using BM2.Contracts.Common;
using FluentValidation;

namespace BM2.Contracts.Records;

/// <summary>Creates or replaces a record. Amounts are never negative; <see cref="Type"/> gives the direction.</summary>
public sealed record SaveRecordRequest(
    RecordType Type,
    RecordStatus Status,
    DateOnly OccurredOn,
    Guid AccountId,
    decimal Amount,
    string Currency,
    decimal AccountAmount,
    Guid? CategoryId,
    Guid? TransferAccountId,
    decimal? TransferAmount,
    string? Name,
    IReadOnlyList<Guid> TagIds,
    RecordPair? Pair = null,
    bool UpdatePair = false);

/// <summary>
/// "Send money": an expense here that, once completed, adds an income to <see cref="AccountId"/> in another wallet.
/// Set on create only; <see cref="Amount"/> is in the target account's currency.
/// </summary>
public sealed record RecordPair(Guid WalletId, Guid AccountId, Guid CategoryId, decimal Amount);

public sealed class SaveRecordRequestValidator : AbstractValidator<SaveRecordRequest>
{
    private const string AmountMessage = "Enter an amount of 0 or more with at most 2 decimal places.";

    public SaveRecordRequestValidator()
    {
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.OccurredOn).Must(d => d.Year is >= 2000 and <= 2100).WithMessage("Choose a date.");
        RuleFor(x => x.Amount).Must(MoneyRules.IsValidAmount).WithMessage(AmountMessage);
        RuleFor(x => x.AccountAmount).Must(MoneyRules.IsValidAmount).WithMessage(AmountMessage);
        RuleFor(x => x.TransferAmount)
            .Must(amount => amount is null || MoneyRules.IsValidAmount(amount.Value)).WithMessage(AmountMessage);
        RuleFor(x => x.Currency).Currency();
        RuleFor(x => x.CategoryId)
            .NotNull().When(x => x.Type != RecordType.Transfer).WithMessage("Choose a category.");
        RuleFor(x => x.TransferAccountId)
            .Must((request, target) => target is not null && target != request.AccountId)
            .When(x => x.Type == RecordType.Transfer)
            .WithMessage("Choose another account to move the money to.");
        RuleFor(x => x.Name)
            .Must(name => name is null || name.Trim().Length <= Limits.RecordName)
            .WithMessage($"Use at most {Limits.RecordName} characters.");
        RuleFor(x => x.Pair)
            .Must((request, pair) => request.Type == RecordType.Expense && MoneyRules.IsValidAmount(pair!.Amount))
            .When(x => x.Pair is not null)
            .WithMessage("Only an expense can send money, with a valid amount received.");
        RuleFor(x => x.TagIds)
            .NotNull()
            .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Each tag can be added once.");
    }
}
