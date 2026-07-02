using BM2.Infrastructure.Repositories.Base;
using BM2.Application.Responses;
using BM2.Shared.Requests.Commands.Record;
using MediatR;

namespace BM2.Application.Functions.Record.Commands;

public class DeleteRecordCommandHandler(UnitOfWork unitOfWork)
    : IRequestHandler<DeleteRecordCommand, BaseResponse>
{
    public async Task<BaseResponse> Handle(DeleteRecordCommand request, CancellationToken cancellationToken)
    {
        var record = await unitOfWork.RecordRepository.GetByIdAsync(request.Id);

        record.ThrowExceptionIfNull();
        record!.CheckPermission(request.OwnedByUserId);

        record.DeletedAt = DateTime.UtcNow;
        record.DeletedBy = request.OwnedByUserId;

        try
        {
            await unitOfWork.RecordRepository.Update(record);
            await unitOfWork.SaveAsync();

            return request.ReturnSuccess();
        }
        catch (Exception)
        {
            return request.ReturnServerError();
        }
    }
}
