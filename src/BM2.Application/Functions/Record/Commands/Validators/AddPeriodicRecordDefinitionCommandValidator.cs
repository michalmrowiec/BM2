using BM2.Infrastructure.Repositories.Base;
using BM2.Application.Responses;
using BM2.Shared.Requests.Commands.Record;
using FluentValidation;

namespace BM2.Application.Functions.Record.Commands.Validators;

public class AddPeriodicRecordDefinitionCommandValidator : AbstractValidator<AddPeriodicRecordDefinitionCommand>
{
    private readonly UnitOfWork _unitOfWork;

    public AddPeriodicRecordDefinitionCommandValidator(UnitOfWork unitOfWork, bool validateLimit = true)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x)
            .CustomAsync(ValidateReferencesAsync);

        if (validateLimit)
        {
            RuleFor(x => x)
                .CustomAsync(ValidateMaxDefinitionsAsync);
        }
    }

    private async Task ValidateReferencesAsync(
        AddPeriodicRecordDefinitionCommand request,
        ValidationContext<AddPeriodicRecordDefinitionCommand> context,
        CancellationToken cancellationToken)
    {
        var wallet = await _unitOfWork.WalletRepository.GetByIdAsync(request.WalletId);
        wallet.ThrowExceptionIfNull();
        wallet!.CheckPermission(request.OwnedByUserId);

        var recordTemplate = await _unitOfWork.RecordTemplateRepository.GetByIdAsync(request.RecordTemplateId);
        recordTemplate.ThrowExceptionIfNull();
        recordTemplate!.CheckPermission(request.OwnedByUserId);

        if (recordTemplate.WalletId != request.WalletId)
        {
            context.AddFailure("Selected template does not belong to the selected wallet.");
        }

        var account = await _unitOfWork.AccountRepository.GetByIdAsync(request.SetRecordAccountId);
        account.ThrowExceptionIfNull();
        account!.CheckPermission(request.OwnedByUserId);

        if (account.WalletId != request.WalletId)
        {
            context.AddFailure("Selected account does not belong to the selected wallet.");
        }

        var currency = await _unitOfWork.CurrencyRepository.GetByIdAsync(request.CurrencyId);
        currency.ThrowExceptionIfNull();

        var periodicStatus = await _unitOfWork.RecordStatusRepository.GetByAsync(x =>
            x.Id == request.PeriodicRecordStatusId && x.ForPeriodicRecord);

        if (periodicStatus == null)
        {
            context.AddFailure("Selected periodic record status is invalid.");
        }

        var setRecordStatus = await _unitOfWork.RecordStatusRepository.GetByAsync(x =>
            x.Id == request.SetRecordStatusId && x.ForRecords);

        if (setRecordStatus == null)
        {
            context.AddFailure("Selected record status is invalid.");
        }
    }

    private async Task ValidateMaxDefinitionsAsync(
        AddPeriodicRecordDefinitionCommand request,
        ValidationContext<AddPeriodicRecordDefinitionCommand> context,
        CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.OwnedByUserId);
        user.ThrowExceptionIfNull();

        var definitions =
            await _unitOfWork.PeriodicRecordDefinitionRepository.GetAllForUserAsync(request.OwnedByUserId);

        if (definitions.Count >= user!.MaxPeriodicRecordDefinitions)
        {
            context.AddFailure(
                $"The user has reached the maximum number of periodic record definitions ({user.MaxPeriodicRecordDefinitions}).");
        }
    }
}
