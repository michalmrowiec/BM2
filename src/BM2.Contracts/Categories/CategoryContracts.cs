using BM2.Contracts.Common;
using FluentValidation;

namespace BM2.Contracts.Categories;

public enum CategoryType
{
    Expense = 1,
    Income = 2,
}

public sealed record CategoryItem(Guid Id, string Name, CategoryType Type, bool IsArchived, string? Icon = null);

public sealed record CreateCategoryRequest(string Name, CategoryType Type, string? Icon = null);

/// <summary>The type is fixed after creation, because records rely on it.</summary>
public sealed record UpdateCategoryRequest(string Name, bool IsArchived, string? Icon = null);

/// <summary>Keys of the category icons drawn by the client's Icon component, in picker order.</summary>
public static class CategoryIcons
{
    public static readonly IReadOnlyList<string> All =
    [
        "cart", "basket", "food", "coffee", "fuel", "car", "bus", "home", "bolt", "water", "wifi", "pill", "gift", "plane",
        "shirt", "kids", "pet", "sport", "film", "book", "beauty", "tools", "briefcase", "piggy", "trend", "receipt",
        "shield", "repeat", "heart", "dots",
    ];

    /// <summary>No icon is valid too.</summary>
    public static bool IsKnown(string? key) => key is null || All.Contains(key);
}

public sealed class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).Name(Limits.CategoryName);
        RuleFor(x => x.Type).IsInEnum().WithMessage("Choose expense or income.");
        RuleFor(x => x.Icon).Must(CategoryIcons.IsKnown).WithMessage("Choose an icon from the list.");
    }
}

public sealed class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).Name(Limits.CategoryName);
        RuleFor(x => x.Icon).Must(CategoryIcons.IsKnown).WithMessage("Choose an icon from the list.");
    }
}
