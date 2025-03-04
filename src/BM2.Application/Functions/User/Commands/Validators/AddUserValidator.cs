﻿using BM2.Shared.Requests.Commands.User;
using BM2.Shared.Requests.Queries.User;
using FluentValidation;
using MediatR;

namespace BM2.Application.Functions.User.Commands.Validators;

public class AddUserValidator : AbstractValidator<AddUserCommand>
{
    public AddUserValidator(IMediator mediator)
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