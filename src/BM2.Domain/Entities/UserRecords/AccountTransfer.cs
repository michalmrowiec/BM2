using BM2.Domain.Entities.Interfaces;

namespace BM2.Domain.Entities.UserRecords;

public class AccountRecordTransfer : IEntity, IEntityAudit, IOwnedByUser
{
    public Guid Id { get; set; }

    public Guid FromRecordId { get; set; }
    public Guid ToRecordId { get; set; }

    public Record FromRecord { get; set; } = null!;
    public Record ToRecord { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    public Guid OwnedByUserId { get; set; }
}