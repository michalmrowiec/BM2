namespace BM2.Shared.DTOs;

public class RecordDTO : BaseRecordDTO
{
    public Guid AccountId { get; set; }
    public DateTime RecordDateTime { get; set; }
    public Guid? AccountRecordTransferId { get; set; }

    public RecordDTO()
    { }

    public RecordDTO(RecordTemplateDTO recordTemplate, Guid? accountId) : base(recordTemplate)
    {
        if(accountId.HasValue)
            AccountId = accountId.Value;
    }
}