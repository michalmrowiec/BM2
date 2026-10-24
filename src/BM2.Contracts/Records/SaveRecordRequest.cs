using BM2.Contracts.Common;
using FluentValidation;

namespace BM2.Contracts.Records;

/// <summary>Creates or replaces a record. Amounts are positive; <see cref="Type"/> gives the direction.</summary>
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
    IReadOnlyList<Guid> TagIds);

public sealed class SaveRecordRequestValidator : AbstractValidator<SaveRecordRequest>
{
    private const string AmountMessage = "Enter an amount above 0 with at most 2 decimal places.";

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
        RuleFor(x => x.Name)
            .Must(name => name is null || name.Trim().Length <= Limits.RecordName)
            .WithMessage($"Use at most {Limits.RecordName} characters.");
        RuleFor(x => x.TagIds)
            .NotNull()
            .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Each tag can be added once.");
    }
}
