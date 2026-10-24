using BM2.Contracts.Common;
using FluentValidation;

namespace BM2.Contracts.Categories;

public enum CategoryType
{
    Expense = 1,
    Income = 2,
}

public sealed record CategoryItem(Guid Id, string Name, CategoryType Type, bool IsArchived);

public sealed record CreateCategoryRequest(string Name, CategoryType Type);

/// <summary>The type is fixed after creation, because records rely on it.</summary>
public sealed record UpdateCategoryRequest(string Name, bool IsArchived);

public sealed class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).Name(Limits.CategoryName);
        RuleFor(x => x.Type).IsInEnum().WithMessage("Choose expense or income.");
    }
}

public sealed class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator() => RuleFor(x => x.Name).Name(Limits.CategoryName);
}
