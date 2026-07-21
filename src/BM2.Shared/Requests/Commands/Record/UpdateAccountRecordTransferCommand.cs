using BM2.Shared.DTOs;

namespace BM2.Shared.Requests.Commands.Record;

public class UpdateAccountRecordTransferCommand : AddAccountRecordTransferCommand
{
    public Guid Id { get; set; }
    
    
    public UpdateAccountRecordTransferCommand()
    {
    }
    
    public UpdateAccountRecordTransferCommand(AccountRecordTransferDTO transferDto)
    {
        Id = transferDto.Id;
        RecordDateTime = transferDto.RecordDateTime;
        FromAccountId = transferDto.FromAccountId;
        ToAccountId = transferDto.ToAccountId;
        FromAmount = transferDto.FromAmount;
        ToAmount = transferDto.ToAmount;
        FromCurrencyId = transferDto.FromCurrencyId;
        ToCurrencyId = transferDto.ToCurrencyId;
        Name =  transferDto.Name;
        Description = transferDto.Description;
    }
}