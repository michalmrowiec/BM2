namespace BM2.Domain.Entities;

public class RecordTemplate : BaseRecord
{
    public Currency Currency { get; set; }
    public Wallet Wallet { get; set; }
    public Category Category { get; set; }
    public List<RecordTagRelation> RecordTagRelations { get; set; } = [];
}