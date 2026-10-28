using BM2.Contracts.Common;
using FluentValidation;

namespace BM2.Contracts.Accounts;

public sealed record CreateAccountRequest(string Name, string Currency, decimal OpeningBalance, AccountKind Kind = AccountKind.Cash,
    bool ExcludeFromTotal = false, bool ShowOnDashboard = true);

public sealed record UpdateAccountRequest(
    string Name, string Currency, decimal OpeningBalance, bool IsArchived, AccountKind Kind = AccountKind.Cash,
    bool ExcludeFromTotal = false, bool ShowOnDashboard = true);

public sealed class CreateAccountRequestValidator : AbstractValidator<CreateAccountRequest>
{
    public CreateAccountRequestValidator()
    {
        RuleFor(x => x.Name).Name(Limits.AccountName);
        RuleFor(x => x.Currency).Currency();
        RuleFor(x => x.OpeningBalance).Balance();
        RuleFor(x => x.Kind).IsInEnum();
    }
}

public sealed class UpdateAccountRequestValidator : AbstractValidator<UpdateAccountRequest>
{
    public UpdateAccountRequestValidator()
    {
        RuleFor(x => x.Name).Name(Limits.AccountName);
        RuleFor(x => x.Currency).Currency();
        RuleFor(x => x.OpeningBalance).Balance();
        RuleFor(x => x.Kind).IsInEnum();
    }
}

/// <summary>PaidIn: the account's net paid-in amount (balance from records) on that day.</summary>
public sealed record AccountValuationItem(Guid Id, DateOnly ValuedOn, decimal Value, decimal PaidIn);

/// <summary>Sets the account's value on a day; a second valuation on the same day replaces the first.</summary>
public sealed record SaveValuationRequest(DateOnly ValuedOn, decimal Value);

public sealed class SaveValuationRequestValidator : AbstractValidator<SaveValuationRequest>
{
    public SaveValuationRequestValidator()
    {
        RuleFor(x => x.ValuedOn).NotEmpty().WithMessage("Choose a date.");
        RuleFor(x => x.Value).GreaterThanOrEqualTo(0).WithMessage("Enter a value of 0 or more.").Balance();
    }
}
