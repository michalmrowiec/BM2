using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Functions.Record.Commands.Requests;
using BM2.Application.Responses;
using BM2.Domain.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BM2.Application.Functions.Record.Commands.Validators;

public class AddRecordCommandValidator : BaseRecordCommandValidator<AddRecordCommand>
{
    public AddRecordCommandValidator(IUnitOfWork unitOfWork) : base()
    {
        RuleFor(x => x)
            .CustomAsync(async (request, context, cancellationToken) =>
            {
                var user = await unitOfWork.UserRepository.GetByIdAsync(request.OwnedByUserId,
                    q => q.Include(u => u.Accounts));
                user.ThrowExceptionIfNull();

                var userAccounts = user!.Accounts.Select(x => x.Id);

                if (!userAccounts.Contains(request.AccountId))
                {
                    throw new DomainExceptions.UnauthenticatedException(
                        $"User {request.OwnedByUserId} does not have access to the following account: {request.AccountId}");
                }
            });

        RuleFor(x => x)
            .CustomAsync(async (request, context, cancellationToken) =>
            {
                var user = await unitOfWork.UserRepository.GetByIdAsync(request.OwnedByUserId);
                user.ThrowExceptionIfNull();
                var maxRecordsPerMonth = user!.MaxRecordsPerMonth;

                var recordForMonth = await unitOfWork.RecordRepository.GetAllForMonthAsync(request.OwnedByUserId,
                    request.RecordDateTime.Year, request.RecordDateTime.Month);

                if (recordForMonth.Count >= maxRecordsPerMonth)
                {
                    context.AddFailure(
                        $"The user has reached the maximum number of record ({maxRecordsPerMonth}) for this month ({request.RecordDateTime.Year}-{request.RecordDateTime.Month}).");
                }
            });
    }
}