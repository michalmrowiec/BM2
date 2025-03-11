using AutoMapper;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Responses;
using BM2.Shared.Requests.Commands.Account;
using MediatR;

namespace BM2.Application.Functions.Account.Commands;

public class UpdateAccountAssignmentCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateAccountAssignmentCommand, BaseResponse>
{
    public async Task<BaseResponse> Handle(UpdateAccountAssignmentCommand request, CancellationToken cancellationToken)
    {
        var oldAccount = await unitOfWork.AccountRepository.GetByIdAsync(request.OldAccountId); 
        var newAccount = await unitOfWork.AccountRepository.GetByIdAsync(request.NewAccountId);
        
        oldAccount.ThrowExceptionIfNull();
        newAccount.ThrowExceptionIfNull();
        oldAccount!.CheckPermission(request.OwnedByUserId);
        newAccount!.CheckPermission(request.OwnedByUserId);
        
        var records = await unitOfWork.RecordRepository.GetListByAsync(x => x.AccountId == oldAccount!.Id);
        
        records.ThrowExceptionIfNull();
        records.CheckPermission(request.OwnedByUserId);
        
        foreach (var record in records)
        {
            record.AccountId = newAccount!.Id;
        }
        
        try
        {
            await unitOfWork.RecordRepository.UpdateRange(records.ToList());
            await unitOfWork.SaveAsync();

            return request.ReturnSuccess();
        }
        catch (Exception e)
        {
            return request.ReturnServerError();
        }
    }
}