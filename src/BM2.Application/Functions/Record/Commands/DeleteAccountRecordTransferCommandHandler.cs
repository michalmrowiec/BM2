using BM2.Infrastructure.Repositories.Base;
using BM2.Application.Responses;
using BM2.Shared.Requests.Commands.Record;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BM2.Application.Functions.Record.Commands;

public class DeleteAccountRecordTransferCommandHandler(UnitOfWork unitOfWork)
    : IRequestHandler<DeleteAccountRecordTransferCommand, BaseResponse>
{
    public async Task<BaseResponse> Handle(DeleteAccountRecordTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = (await unitOfWork.AccountRecordTransferRepository.GetByIdAsync(
            request.Id,
            x => x
                .Include(t => t.FromRecord)
                .Include(t => t.ToRecord))).EnsureFound();
        
        transfer!.CheckPermission(request.OwnedByUserId);

        transfer.DeletedAt = DateTime.UtcNow;
        transfer.DeletedBy = request.OwnedByUserId;

        transfer.FromRecord.DeletedAt = DateTime.UtcNow;
        transfer.FromRecord.DeletedBy = request.OwnedByUserId;
        
        transfer.ToRecord.DeletedAt = DateTime.UtcNow;
        transfer.ToRecord.DeletedBy = request.OwnedByUserId;
        
        try
        {
            await unitOfWork.AccountRecordTransferRepository.Update(transfer);
            await unitOfWork.SaveAsync();

            return request.ReturnSuccess();
        }
        catch (Exception)
        {
            return request.ReturnServerError();
        }
    }
}
