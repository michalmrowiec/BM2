using BM2.Application.Mappings;
using BM2.Application.Responses;
using BM2.Domain.Entities.UserRecords;
using BM2.Infrastructure.Repositories.Base;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.Record;
using BM2.Shared.SystemCodes;
using MediatR;

namespace BM2.Application.Functions.Record.Commands;

public class AddAccountRecordTransferHandler(UnitOfWork unitOfWork)
    : IRequestHandler<AddAccountRecordTransferCommand, BaseResponse<AccountRecordTransferDTO>>
{
    public async Task<BaseResponse<AccountRecordTransferDTO>> Handle(AddAccountRecordTransferCommand request,
        CancellationToken cancellationToken)
    {
        // var validationResult =
        //     await new AddRecordCommandValidator(unitOfWork).ValidateAsync(request, cancellationToken);

        // if (!validationResult.IsValid) return new BaseResponse<RecordDTO>(validationResult);

        var realizedStatusId =
            (await unitOfWork.RecordStatusRepository.GetByAsync(x => x.SystemCode == StatusSystemCode.Realized))
            .EnsureFound().Id;

        var fromRecord = request.ToFromTransferRecordEntity(realizedStatusId);
        var toRecord = request.ToToTransferRecordEntity(realizedStatusId);

        var transfer = new AccountRecordTransfer()
        {
            Id = Guid.NewGuid(),
            FromRecord = fromRecord,
            ToRecord = toRecord,
            OwnedByUserId = request.OwnedByUserId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.OwnedByUserId,
        };

        fromRecord.AccountRecordTransferId = transfer.Id;
        toRecord.AccountRecordTransferId = transfer.Id;

        try
        {
            transfer = await unitOfWork.AccountRecordTransferRepository.Add(transfer);
            await unitOfWork.SaveAsync();

            return request.ReturnSuccessWithObject(transfer.ToDto());
        }
        catch (Exception e)
        {
            return request.ReturnServerError();
        }
    }
}