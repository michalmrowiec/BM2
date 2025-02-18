using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Responses;
using BM2.Shared.Models;
using BM2.Shared.Requests.Commands.Category;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BM2.Application.Functions.Category.Commands.Validators;

public class AddCategoryCommandValidator : AbstractValidator<AddCategoryCommand>
{
    public AddCategoryCommandValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.CategoryName)
            .NotEmpty()
            .MaximumLength(ModelsRequirements.CategoryNameMaxLength);

        RuleFor(x => x)
            .CustomAsync(async (request, context, cancellationToken) =>
            {
                await request.WalletIds.ValidateAllWalletsBelongToUser(request.OwnedByUserId, unitOfWork, cancellationToken);
            });

        RuleFor(x => x)
            .CustomAsync(async (request, context, cancellationToken) =>
            {
                var user = await unitOfWork.UserRepository.GetByIdAsync(request.OwnedByUserId,
                    q => q.Include(u => u.Categories));
                user.ThrowExceptionIfNull();

                var maxCategories = user!.MaxCategories;

                if (user.Categories.Count >= maxCategories)
                {
                    context.AddFailure($"The user has reached the maximum number of categories ({maxCategories}).");
                }
            });
    }
}