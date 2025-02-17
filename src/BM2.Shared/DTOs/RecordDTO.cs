namespace BM2.Shared.DTOs;

public class RecordDTO : BaseRecordDTO
{
    public Guid AccountId { get; set; }
    public DateTime RecordDateTime { get; set; }
}