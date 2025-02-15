using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Functions.Record.Commands.Requests;
using BM2.Application.Responses;
using BM2.Domain.Entities.UserRecords;
using FluentValidation;

namespace BM2.Application.Functions.Record.Commands.Validators;

public abstract class AddBaseRecordCommandValidator<TCommand> : AbstractValidator<TCommand>
    where TCommand : AddBaseRecordCommand
{
    protected AddBaseRecordCommandValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(BaseRecord.RecordNameMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(BaseRecord.RecordDescriptionMaxLength);

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