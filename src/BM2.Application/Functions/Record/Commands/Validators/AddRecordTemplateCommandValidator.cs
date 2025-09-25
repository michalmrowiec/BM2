using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Responses;
using BM2.Shared.Requests.Commands.Record;
using FluentValidation;

namespace BM2.Application.Functions.Record.Commands.Validators;

public class AddRecordTemplateCommandValidator : AddBaseRecordCommandValidator<AddRecordTemplateCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddRecordTemplateCommandValidator(IUnitOfWork unitOfWork, bool validateRecordPerMonthLimit = true) : base(unitOfWork)
    {
        _unitOfWork = unitOfWork;
        
         RuleFor(x => x)
             .CustomAsync(ValidateWalletCategoryRelationAsync);
        
         RuleFor(x => x)
             .CustomAsync(ValidateWalletTagRelationsAsync);
        
         if (validateRecordPerMonthLimit)
             RuleFor(x => x)
                 .CustomAsync(ValidateMaxRecordsPerMonthAsync);
    }
    
    private async Task ValidateWalletCategoryRelationAsync(
        AddRecordTemplateCommand request,
        ValidationContext<AddRecordTemplateCommand> context,
        CancellationToken cancellationToken)
    {
        var relation = await _unitOfWork.WalletCategoryRelationRepository.GetRelationForWalletAsync(
            request.OwnedByUserId,
            request.WalletId,
            request.CategoryId
        );

        if (relation == null)
        {
            context.AddFailure(
                $"Category {request.CategoryId} cannot be added to wallet {request.WalletId} due to missing wallet relations."
            );
            return;
        }

        if (!relation!.IsActive)
        {
            context.AddFailure(
                $"Category {request.CategoryId} cannot be added to wallet {request.WalletId} because the associated wallet relation is inactive.");
        }
    }

    private async Task ValidateWalletTagRelationsAsync(
        AddRecordTemplateCommand request,
        ValidationContext<AddRecordTemplateCommand> context,
        CancellationToken cancellationToken)
    {
        var relations = await _unitOfWork.WalletTagRelationRepository.GetRelationForWalletAsync(
            request.OwnedByUserId,
            request.WalletId,
            request.TagIds
        );

        var missedRelations = request.TagIds.Except(relations.Select(r => r.TagId)).ToList();

        if (missedRelations.Any())
        {
            context.AddFailure(
                $"Tags {string.Join(", ", missedRelations)} cannot be added to wallet {request.WalletId} due to missing wallet relations."
            );
        }
    }

    private async Task ValidateMaxRecordsPerMonthAsync(
        AddRecordTemplateCommand request,
        ValidationContext<AddRecordTemplateCommand> context,
        CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.OwnedByUserId);
        user.ThrowExceptionIfNull();
        var maxRecordTemplates = user!.MaxRecordTemplates;

        var recordTemplates = await _unitOfWork.RecordTemplateRepository.GetAllForUserAsync(request.OwnedByUserId);

        if (recordTemplates.Count >= maxRecordTemplates)
        {
            context.AddFailure(
                $"The user has reached the maximum number of template records ({maxRecordTemplates}).");
        }
    }
}