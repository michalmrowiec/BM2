using BM2.Application.Contracts.Persistence;
using BM2.Application.Functions.Users.Commands.AddUserCommand;
using BM2.Application.Functions.Users.Queries.GetUserByEmailQuery;
using FluentValidation;
using MediatR;

namespace BM2.Application.Functions.Validators;

public class CreateUserValidator : AbstractValidator<AddUserCommand>
{
    public CreateUserValidator(IMediator mediator)
    {
        RuleFor(x => x.EmailAddress)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255)
            .CustomAsync(async (value, context, cancellationToken) =>
            {
                var user = await mediator.Send(new GetUserByEmailAddressQuery(value), cancellationToken);
                if (user.IsSuccess)
                {
                    context.AddFailure("EmailAddress", "Email address already exists");
                }
            });

        RuleFor(r => r.Password)
            .NotEmpty()
            .MinimumLength(4)
            .MaximumLength(60);

        RuleFor(r => r.RepeatPassword)
            .Equal(r => r.Password)
            .WithMessage("Passwords are not the same");
    }
}