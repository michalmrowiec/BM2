namespace BM2.Domain.Entities;

public class Tag
{
    public Guid TagId { get; set; }
    public string TagName { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    
    public List<RecordTagRelation> RecordTagRelations { get; set; } = [];
    public List<WalletTagRelation> WalletTagRelations { get; set; } = [];
}