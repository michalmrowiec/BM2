using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Responses;
using BM2.Shared.Requests.Commands.Tag;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BM2.Application.Functions.Tag.Commands.Validators;

public class AddTagCommandValidator : AbstractValidator<AddTagCommand>
{
    public AddTagCommandValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.TagName)
            .NotEmpty()
            .MaximumLength(Domain.Entities.UserProfile.Tag.TagNameMaxLength);

        RuleFor(x => x)
            .CustomAsync(async (request, context, cancellationToken) =>
            {
                await request.WalletIds.ValidateAllWalletsBelongToUser(request.OwnedByUserId, unitOfWork,
                    cancellationToken);
            });

        RuleFor(x => x)
            .CustomAsync(async (request, context, cancellationToken) =>
            {
                var user = await unitOfWork.UserRepository.GetByIdAsync(request.OwnedByUserId,
                    q => q.Include(u => u.Tags));
                user.ThrowExceptionIfNull();

                var maxTags = user!.MaxTags;

                if (user.Tags.Count >= maxTags)
                {
                    context.AddFailure($"The user has reached the maximum number of tags ({maxTags}).");
                }
            });
    }
}