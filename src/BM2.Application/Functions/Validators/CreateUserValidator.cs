using BM2.Application.Functions.Commands;
using FluentValidation;

namespace BM2.Application.Functions.Validators;

public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        // TODO Validate email address
        
        RuleFor(r => r.Password)
            .NotEmpty()
            .MinimumLength(4)
            .MaximumLength(60);

        RuleFor(r => r.RepeatPassword)
            .Equal(r => r.Password)
            .WithMessage("Passwords are not the same");
    }
}