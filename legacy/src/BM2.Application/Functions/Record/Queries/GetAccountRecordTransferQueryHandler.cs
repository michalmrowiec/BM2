using BM2.Application.Mappings;
using BM2.Infrastructure.Repositories.Base;
using BM2.Application.Responses;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Queries.Record;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BM2.Application.Functions.Record.Queries;

public class GetAccountRecordTransferQueryHandler(UnitOfWork unitOfWork)
    : IRequestHandler<GetAccountRecordTransferQuery, BaseResponse<AccountRecordTransferDTO>>
{
    public async Task<BaseResponse<AccountRecordTransferDTO>> Handle(
        GetAccountRecordTransferQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var transfer = (await unitOfWork.AccountRecordTransferRepository.GetByIdAsync(
                request.Id,
                x => x
                    .Include(t => t.FromRecord)
                    .Include(t => t.ToRecord))).EnsureFound();
            
            transfer?.CheckPermission(request.UserId);

            return request.ReturnSuccessWithObject(transfer.ToDto());
        }
        catch (Exception e)
        {
            return new BaseResponse<AccountRecordTransferDTO>(BaseResponse.ResponseStatus.ServerError, e.Message);
        }
    }
}