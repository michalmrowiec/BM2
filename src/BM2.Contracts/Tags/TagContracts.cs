using BM2.Contracts.Common;
using FluentValidation;

namespace BM2.Contracts.Tags;

public sealed record TagItem(Guid Id, string Name, bool IsArchived, string? Color = null);

public sealed record CreateTagRequest(string Name, string? Color = null);

public sealed record UpdateTagRequest(string Name, bool IsArchived, string? Color = null);

/// <summary>Palette keys; the client maps each to a theme token (--tag-red etc.), so colors follow the theme.</summary>
public static class TagColors
{
    public static readonly IReadOnlyList<string> All = ["red", "orange", "amber", "green", "teal", "blue", "violet", "pink"];

    /// <summary>No color is valid too.</summary>
    public static bool IsKnown(string? key) => key is null || All.Contains(key);
}

public sealed class CreateTagRequestValidator : AbstractValidator<CreateTagRequest>
{
    public CreateTagRequestValidator()
    {
        RuleFor(x => x.Name).Name(Limits.TagName);
        RuleFor(x => x.Color).Must(TagColors.IsKnown).WithMessage("Choose a color from the palette.");
    }
}

public sealed class UpdateTagRequestValidator : AbstractValidator<UpdateTagRequest>
{
    public UpdateTagRequestValidator()
    {
        RuleFor(x => x.Name).Name(Limits.TagName);
        RuleFor(x => x.Color).Must(TagColors.IsKnown).WithMessage("Choose a color from the palette.");
    }
}
