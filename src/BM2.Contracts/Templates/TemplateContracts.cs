using BM2.Contracts.Common;
using BM2.Contracts.Records;
using FluentValidation;

namespace BM2.Contracts.Templates;

/// <summary>A saved starting point for a record. Fields pointing to archived or missing items come back empty.</summary>
public sealed record TemplateItem(
    Guid Id,
    RecordType Type,
    Guid? AccountId,
    Guid? CategoryId,
    Guid? TransferAccountId,
    decimal? Amount,
    string? Currency,
    string? Name,
    IReadOnlyList<Guid> TagIds,
    bool IsPinned);

/// <summary>Creates or replaces a template; every field except the type is optional.</summary>
public sealed record SaveTemplateRequest(
    RecordType Type,
    Guid? AccountId,
    Guid? CategoryId,
    Guid? TransferAccountId,
    decimal? Amount,
    string? Currency,
    string? Name,
    IReadOnlyList<Guid> TagIds,
    bool IsPinned);

public sealed class SaveTemplateRequestValidator : AbstractValidator<SaveTemplateRequest>
{
    public SaveTemplateRequestValidator()
    {
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Amount)
            .Must(amount => amount is null || MoneyRules.IsValidAmount(amount.Value))
            .WithMessage("Enter an amount of 0 or more with at most 2 decimal places.");
        RuleFor(x => x.Currency).Must(c => c is null || Currencies.IsSupported(c)).WithMessage("Choose a supported currency.");
        RuleFor(x => x.Name)
            .Must(name => name is null || name.Trim().Length <= Limits.RecordName)
            .WithMessage($"Use at most {Limits.RecordName} characters.");
        RuleFor(x => x.TagIds)
            .NotNull()
            .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Each tag can be added once.");
    }
}
