using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Responses;
using BM2.Domain.Entities.UserRecords;
using BM2.Shared.Models;
using BM2.Shared.Requests.Commands.Record;
using FluentValidation;

namespace BM2.Application.Functions.Record.Commands.Validators;

public abstract class AddBaseRecordCommandValidator<TCommand> : AbstractValidator<TCommand>
    where TCommand : AddBaseRecordCommand
{
    protected AddBaseRecordCommandValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(ModelsRequirements.RecordNameMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(ModelsRequirements.RecordDescriptionMaxLength);

        RuleFor(x => x)
            .CustomAsync(async (request, context, cancellationToken) =>
            {
                var category = await unitOfWork.CategoryRepository.GetByIdAsync(request.CategoryId);
                category.ThrowExceptionIfNull();
                category!.CheckPermission(request.OwnedByUserId);
            });

        RuleFor(x => x)
            .CustomAsync(async (request, context, cancellationToken) =>
            {
                var tags = await unitOfWork.TagRepository.GetByIdsAsync(request.TagIds);
                tags.ThrowExceptionIfNull();
                tags!.CheckPermission(request.OwnedByUserId);
            });
    }
}