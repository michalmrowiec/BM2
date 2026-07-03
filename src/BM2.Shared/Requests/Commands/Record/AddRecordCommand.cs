using BM2.Shared.DTOs;

namespace BM2.Shared.Requests.Commands.Record;

public class AddRecordCommand : AddBaseRecordCommand, IBaseRequest<RecordDTO>
{
    public Guid AccountId { get; set; }
    public DateTime RecordDateTime { get; set; } = DateTime.Now;
    
    public AddRecordCommand()
    {
    }

    public AddRecordCommand(RecordDTO record)
    {
        Name = record.Name;
        Amount = record.Amount;
        PlannedAmount = record.PlannedAmount;
        AccountId = record.AccountId;
        CategoryId = record.CategoryId;
        CurrencyId = record.CurrencyId;
        StatusId = record.StatusId;
        Description = record.Description;
        RecordDateTime = record.RecordDateTime;
        TagIds = record.Tags.Select(t => t.Id).ToList();
    }
}