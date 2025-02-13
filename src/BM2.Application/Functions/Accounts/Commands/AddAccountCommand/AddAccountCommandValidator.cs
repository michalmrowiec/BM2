using BM2.Domain.Entities.UserProfile;
using FluentValidation;

namespace BM2.Application.Functions.Accounts.Commands.AddAccountCommand;

public class AddAccountCommandValidator : AbstractValidator<AddAccountCommand>
{
    public AddAccountCommandValidator()
    {
        RuleFor(x => x.AccountName)
            .NotEmpty()
            .MaximumLength(Account.AccountNameMaxLength);
    }
}