namespace BM2.Domain.Entities;

public class Record : BaseRecord
{
    public DateTime RecordDateTime { get; set; }

    public Currency Currency { get; set; }
    public Account Account { get; set; }
    public Category Category { get; set; }
    public List<RecordTagRelation> RecordTagRelations { get; set; } = [];
}