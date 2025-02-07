using BM2.Domain.Entities.Interfaces;
using BM2.Domain.Entities.System;
using BM2.Domain.Entities.UserRecords;

namespace BM2.Domain.Entities.UserProfile
{
    public class Wallet : IEntity, IEntityAudit, IOwnedByUser
    {
        public Guid Id { get; set; }
        public string WalletName { get; set; } = null!;
        public bool IsActive { get; set; }
        public Guid DefaultCurrencyId { get; set; }
        public Guid OwnedByUserId { get; set; }

        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }

        public User? OwnedByUser { get; set; }
        public Currency? DefaultCurrency { get; set; }
        public ICollection<Account> Accounts { get; set; } = [];
        public ICollection<RecordTemplate> RecordTemplates { get; set; } = [];
        public ICollection<PeriodicRecordDefinition> PeriodicRecordDefinitions { get; set; } = [];
        public ICollection<Category> Categories { get; set; } = [];
        public ICollection<Tag> Tags { get; set; } = [];

        public static int WalletNameMaxLength => 100;
    }
}