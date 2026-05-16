using BM2.Application.Responses;
using BM2.Infrastructure.Repositories.Base;
using BM2.Shared.Requests.Commands.Account;
using MediatR;

namespace BM2.Application.Functions.Account.Commands;

public class UpdateAccountAssignmentCommandHandler(UnitOfWork unitOfWork)
    : IRequestHandler<UpdateAccountAssignmentCommand, BaseResponse>
{
    public async Task<BaseResponse> Handle(UpdateAccountAssignmentCommand request,
        CancellationToken cancellationToken)
    {
        var oldAccount = await unitOfWork.AccountRepository.GetByIdAsync(request.OldAccountId);
        var newAccount = await unitOfWork.AccountRepository.GetByIdAsync(request.NewAccountId);

        oldAccount.ThrowExceptionIfNull();
        oldAccount!.CheckPermission(request.OwnedByUserId);

        newAccount.ThrowExceptionIfNull();
        newAccount!.CheckPermission(request.OwnedByUserId);

        var records = await unitOfWork.RecordRepository.GetListByAsync(x => x.AccountId == oldAccount.Id);

        foreach (var record in records)
        {
            record.AccountId = newAccount.Id;
            record.UpdatedAt = DateTime.UtcNow;
            record.UpdatedBy = request.OwnedByUserId;
        }

        try
        {
            await unitOfWork.RecordRepository.UpdateRange(records.ToList());
            await unitOfWork.AccountRepository.Delete(oldAccount);
            await unitOfWork.SaveAsync();

            return request.ReturnSuccess();
        }
        catch (Exception)
        {
            return request.ReturnServerError();
        }
    }
}
