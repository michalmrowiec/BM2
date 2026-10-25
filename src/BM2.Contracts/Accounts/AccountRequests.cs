using BM2.Contracts.Common;
using FluentValidation;

namespace BM2.Contracts.Accounts;

public sealed record CreateAccountRequest(string Name, string Currency, decimal OpeningBalance);

public sealed record UpdateAccountRequest(string Name, string Currency, decimal OpeningBalance, bool IsArchived);

public sealed class CreateAccountRequestValidator : AbstractValidator<CreateAccountRequest>
{
    public CreateAccountRequestValidator()
    {
        RuleFor(x => x.Name).Name(Limits.AccountName);
        RuleFor(x => x.Currency).Currency();
        RuleFor(x => x.OpeningBalance).Balance();
    }
}

public sealed class UpdateAccountRequestValidator : AbstractValidator<UpdateAccountRequest>
{
    public UpdateAccountRequestValidator()
    {
        RuleFor(x => x.Name).Name(Limits.AccountName);
        RuleFor(x => x.Currency).Currency();
        RuleFor(x => x.OpeningBalance).Balance();
    }
}
