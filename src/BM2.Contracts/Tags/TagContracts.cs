using BM2.Contracts.Common;
using FluentValidation;

namespace BM2.Contracts.Tags;

public sealed record TagItem(Guid Id, string Name, bool IsArchived);

public sealed record CreateTagRequest(string Name);

public sealed record UpdateTagRequest(string Name, bool IsArchived);

public sealed class CreateTagRequestValidator : AbstractValidator<CreateTagRequest>
{
    public CreateTagRequestValidator() => RuleFor(x => x.Name).Name(Limits.TagName);
}

public sealed class UpdateTagRequestValidator : AbstractValidator<UpdateTagRequest>
{
    public UpdateTagRequestValidator() => RuleFor(x => x.Name).Name(Limits.TagName);
}
