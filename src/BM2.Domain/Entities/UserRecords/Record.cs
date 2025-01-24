namespace BM2.Domain.Entities;

public class Record : BaseRecord
{
    public Guid AccountId { get; set; }
    public DateTime RecordDateTime { get; set; }

    public Account? Account { get; set; }
}