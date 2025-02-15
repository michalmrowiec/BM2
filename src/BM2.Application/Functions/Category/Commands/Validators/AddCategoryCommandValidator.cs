using BM2.Application.Functions.Category.Commands.Requests;
using FluentValidation;

namespace BM2.Application.Functions.Category.Commands.Validators;

public class AddCategoryCommandValidator : AbstractValidator<AddCategoryCommand>
{
    public AddCategoryCommandValidator()
    {
        RuleFor(x => x.CategoryName)
            .NotEmpty()
            .MaximumLength(Domain.Entities.UserProfile.Category.CategoryNameMaxLength);
    }
}