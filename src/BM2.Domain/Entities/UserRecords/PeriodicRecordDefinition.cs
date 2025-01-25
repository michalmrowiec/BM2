namespace BM2.Domain.Entities;

public class PeriodicRecordDefinition : IEntity, IEntityAudit, IOwnedByUser
{
    public Guid Id { get; set; }
    public Guid OwnedByUserId { get; set; }
    public Guid RecordTemplateId { get; set; }
    public Guid CurrencyId { get; set; }
    public Guid PeriodicRecordStatusId { get; set; } // Status of periodic record
    public Guid SetRecordStatusId { get; set; } // Set status for created record
    public Guid WalletId { get; set; }
    public Guid SetRecordAccountId { get; set; } // Set account for created record

    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public User? OwnedByUser { get; set; }
    public Wallet? Wallet { get; set; }
    public Account? SetRecordAccount { get; set; }
    public Currency? Currency { get; set; }
    public RecordTemplate? RecordTemplate { get; set; }
    public RecordStatus? PeriodicRecordStatus { get; set; }
    public RecordStatus? SetRecordStatus { get; set; }
}