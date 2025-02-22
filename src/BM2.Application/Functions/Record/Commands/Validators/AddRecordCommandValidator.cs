using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Responses;
using BM2.Domain.Exceptions;
using BM2.Shared.Requests.Commands.Record;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BM2.Application.Functions.Record.Commands.Validators;

public class AddRecordCommandValidator : AddBaseRecordCommandValidator<AddRecordCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddRecordCommandValidator(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x)
            .CustomAsync(ValidateAccountAccessAsync);

        RuleFor(x => x)
            .CustomAsync(ValidateWalletCategoryRelationAsync);

        RuleFor(x => x)
            .CustomAsync(ValidateWalletTagRelationsAsync);

        RuleFor(x => x)
            .CustomAsync(ValidateMaxRecordsPerMonthAsync);
    }

    private async Task ValidateAccountAccessAsync(
        AddRecordCommand request,
        ValidationContext<AddRecordCommand> context,
        CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(
            request.OwnedByUserId,
            q => q.Include(u => u.Accounts));
        user.ThrowExceptionIfNull();

        var userAccounts = user!.Accounts.Select(x => x.Id);

        if (!userAccounts.Contains(request.AccountId))
        {
            throw new DomainExceptions.UnauthenticatedException(
                $"User {request.OwnedByUserId} does not have access to the following account: {request.AccountId}");
        }
    }

    private async Task ValidateWalletCategoryRelationAsync(
        AddRecordCommand request,
        ValidationContext<AddRecordCommand> context,
        CancellationToken cancellationToken)
    {
        var relation = await _unitOfWork.WalletCategoryRelationRepository.GetRelationForAccountAsync(
            request.OwnedByUserId,
            request.AccountId,
            request.CategoryId
        );

        if (relation == null)
        {
            context.AddFailure(
                $"Category {request.CategoryId} cannot be added to account {request.AccountId} due to missing wallet relations."
            );
            return;
        }

        if (!relation!.IsActive)
        {
            context.AddFailure(
                $"Category {request.CategoryId} cannot be added to account {request.AccountId} because the associated wallet relation is inactive.");
        }
    }

    private async Task ValidateWalletTagRelationsAsync(
        AddRecordCommand request,
        ValidationContext<AddRecordCommand> context,
        CancellationToken cancellationToken)
    {
        var relations = await _unitOfWork.WalletTagRelationRepository.GetRelationForAccountAsync(
            request.OwnedByUserId,
            request.AccountId,
            request.TagIds
        );

        var missedRelations = request.TagIds.Except(relations.Select(r => r.TagId)).ToList();

        if (missedRelations.Any())
        {
            context.AddFailure(
                $"Tags {string.Join(", ", missedRelations)} cannot be added to account {request.AccountId} due to missing wallet relations."
            );
        }
    }

    private async Task ValidateMaxRecordsPerMonthAsync(
        AddRecordCommand request,
        ValidationContext<AddRecordCommand> context,
        CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.OwnedByUserId);
        user.ThrowExceptionIfNull();
        var maxRecordsPerMonth = user!.MaxRecordsPerMonth;

        var recordForMonth = await _unitOfWork.RecordRepository.GetAllForMonthAsync(request.OwnedByUserId,
            request.RecordDateTime.Year, request.RecordDateTime.Month);

        if (recordForMonth.Count >= maxRecordsPerMonth)
        {
            context.AddFailure(
                $"The user has reached the maximum number of record ({maxRecordsPerMonth}) for this month ({request.RecordDateTime.Year}-{request.RecordDateTime.Month}).");
        }
    }
}