using BM2.Domain.Entities.Interfaces;
using BM2.Domain.Entities.System;
using BM2.Domain.Entities.UserProfile;
using BM2.Shared.SystemCodes;

namespace BM2.Domain.Entities.UserRecords;

public class PeriodicRecordDefinition : IEntity, IEntityAudit, IOwnedByUser
{
    public Guid Id { get; set; }
    public Guid RecordTemplateId { get; set; }
    public Guid CurrencyId { get; set; }
    public Guid PeriodicRecordStatusId { get; set; } // Status of periodic record
    public Guid SetRecordStatusId { get; set; } // Set status for created record
    public Guid WalletId { get; set; }
    public Guid SetRecordAccountId { get; set; } // Set account for created record
    public Guid OwnedByUserId { get; set; }

    public Periodicity Periodicity { get; set; } // Monthly, Yearly
    public DateTime StartDate { get; set; } // Data pierwszej płatności (Anchor Date)

    // To pole jest kluczowe dla jasności w UI
    public DateTime? NextExecutionAt { get; set; }

    public string? HangfireJobId { get; set; }

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