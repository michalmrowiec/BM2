namespace BM2.Domain.Entities;

public class PeriodicRecordDefinition
{
    public Guid PeriodicRecordDefinitionId { get; set; }
    public Guid UserId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}