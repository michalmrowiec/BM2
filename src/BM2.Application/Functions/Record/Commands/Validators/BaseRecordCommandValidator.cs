using BM2.Application.Functions.Record.Commands.Requests;
using BM2.Domain.Entities.UserRecords;
using FluentValidation;

namespace BM2.Application.Functions.Record.Commands.Validators;

public abstract class BaseRecordCommandValidator<TCommand> : AbstractValidator<TCommand> where TCommand : AddBaseRecordCommand
{
    protected BaseRecordCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(BaseRecord.RecordNameMaxLength);
        
        RuleFor(x => x.Description)
            .MaximumLength(BaseRecord.RecordDescriptionMaxLength);
    }
}