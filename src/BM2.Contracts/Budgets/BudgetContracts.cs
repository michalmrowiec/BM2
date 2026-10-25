using BM2.Contracts.Common;
using FluentValidation;

namespace BM2.Contracts.Budgets;

/// <summary>Budgets for one month. Spent counts completed expenses on accounts in the wallet's base currency.</summary>
public sealed record BudgetOverview(DateOnly Month, string Currency, IReadOnlyList<BudgetItem> Items);

/// <param name="OtherSpent">Spending in the category on accounts in other currencies, not counted against the limit.</param>
public sealed record BudgetItem(Guid Id, Guid CategoryId, decimal Limit, decimal Spent, decimal Planned, IReadOnlyList<CurrencyAmount> OtherSpent);

public sealed record CurrencyAmount(string Currency, decimal Amount);

/// <summary>Sets the monthly limit of an expense category (creates the budget when there is none).</summary>
public sealed record SaveBudgetRequest(Guid CategoryId, decimal Limit);

public sealed class SaveBudgetRequestValidator : AbstractValidator<SaveBudgetRequest>
{
    public SaveBudgetRequestValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Choose a category.");
        RuleFor(x => x.Limit)
            .Must(limit => limit > 0 && MoneyRules.IsValidAmount(limit))
            .WithMessage("Enter an amount above 0 with at most 2 decimal places.");
    }
}
