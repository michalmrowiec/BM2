using BM2.Domain.Entities.Interfaces;
using BM2.Domain.Entities.System;
using BM2.Domain.Entities.UserProfile;

namespace BM2.Domain.Entities.UserRecords;

public abstract class BaseRecord : IEntity, IEntityAudit, IOwnedByUser
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public Guid StatusId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public decimal? PlannedAmount { get; set; }
    public Guid CurrencyId { get; set; }
    public Guid OwnedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    
    public Currency? Currency { get; set; }
    public Category? Category { get; set; }
    public User? OwnedByUser { get; set; }
    public RecordStatus? Status { get; set; }
    public ICollection<Tag> Tags { get; set; } = [];
    
    public static int RecordNameMaxLength => 150;
    public static int RecordDescriptionMaxLength => 500;
}