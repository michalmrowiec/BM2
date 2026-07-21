using BM2.Application.Mappings;
using BM2.Application.Responses;
using BM2.Infrastructure.Repositories.Base;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.Record;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BM2.Application.Functions.Record.Commands;

public class UpdateAccountRecordTransferHandler(UnitOfWork unitOfWork)
    : IRequestHandler<UpdateAccountRecordTransferCommand, BaseResponse<AccountRecordTransferDTO>>
{
    public async Task<BaseResponse<AccountRecordTransferDTO>> Handle(UpdateAccountRecordTransferCommand request,
        CancellationToken cancellationToken)
    {
        // var validationResult =
        //     await new AddRecordCommandValidator(unitOfWork).ValidateAsync(request, cancellationToken);

        // if (!validationResult.IsValid) return new BaseResponse<RecordDTO>(validationResult);


        var transfer = (await unitOfWork.AccountRecordTransferRepository.GetByIdAsync(
            request.Id,
            x => x
                .Include(t => t.FromRecord)
                .Include(t => t.ToRecord))).EnsureFound();

        transfer.UpdatedAt = DateTime.UtcNow;
        transfer.UpdatedBy = request.OwnedByUserId;

        transfer.FromRecord.UpdatedAt = DateTime.UtcNow;
        transfer.FromRecord.UpdatedBy = request.OwnedByUserId;
        transfer.FromRecord.AccountId = request.FromAccountId;
        transfer.FromRecord.RecordDateTime = request.RecordDateTime;
        transfer.FromRecord.Name = request.Name;
        transfer.FromRecord.Description = request.Description;
        transfer.FromRecord.AccountAmount = Math.Abs(request.FromAmount) * -1;
        transfer.FromRecord.Amount = Math.Abs(request.FromAmount) * -1;
        transfer.FromRecord.CurrencyId = request.FromCurrencyId;

        transfer.ToRecord.UpdatedAt = DateTime.UtcNow;
        transfer.ToRecord.UpdatedBy = request.OwnedByUserId;
        transfer.ToRecord.AccountId = request.ToAccountId;
        transfer.ToRecord.RecordDateTime = request.RecordDateTime;
        transfer.ToRecord.Name = request.Name;
        transfer.ToRecord.Description = request.Description;
        transfer.ToRecord.AccountAmount = Math.Abs(request.ToAmount);
        transfer.ToRecord.Amount = Math.Abs(request.ToAmount);
        transfer.ToRecord.CurrencyId = request.ToCurrencyId;

        try
        {
            transfer = await unitOfWork.AccountRecordTransferRepository.Update(transfer);
            await unitOfWork.SaveAsync();

            return request.ReturnSuccessWithObject(transfer.ToDto());
        }
        catch (Exception e)
        {
            return request.ReturnServerError();
        }
    }
}