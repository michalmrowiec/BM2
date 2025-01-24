namespace BM2.Domain.Entities;

public class Tag : IEntity, IEntityAudit, IOwnedByUser
{
    public Guid Id { get; set; }
    public string TagName { get; set; }
    public Guid OwnedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    
    public List<RecordTagRelation> RecordTagRelations { get; set; } = [];
    public List<WalletTagRelation> WalletTagRelations { get; set; } = [];
}