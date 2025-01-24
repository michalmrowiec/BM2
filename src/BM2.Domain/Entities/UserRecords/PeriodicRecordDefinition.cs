namespace BM2.Domain.Entities;

public class PeriodicRecordDefinition : IEntity, IEntityAudit, IOwnedByUser
{
    public Guid Id { get; set; }
    public Guid OwnedByUserId { get; set; }
    public Guid RecordTemplateId { get; set; }
    //public Guid RecordStatusId { get; set; } //?
    public Guid SetRecordStatusId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    
    public RecordTemplate? RecordTemplate { get; set; }
    //public RecordStatus RecordStatus { get; set; }
    public RecordStatus? SetRecordStatus { get; set; }
}