namespace BM2.Application.DTOs;

public class RecordDTO : BaseRecordDTO
{
    public Guid AccountId { get; set; }
    public DateTime RecordDateTime { get; set; }
}