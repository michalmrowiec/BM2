using BM2.Domain.Entities.Interfaces;
using BM2.Domain.Entities.UserRecords;

namespace BM2.Domain.Entities.UserProfile
{
    public class User : IEntity, IEntityAudit
    {
        public Guid Id { get; set; }
        public string EmailAddress { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public bool IsActive { get; set; }
        public int MaxCategories { get; set; }
        public int MaxTags { get; set; }
        public int MaxRecordTemplates { get; set; }
        public int MaxPeriodicRecordDefinitions { get; set; }
        public int MaxRecordsPerMonth { get; set; }
        public int MaxWallets { get; set; }
        public int MaxAccountsPerWallet { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        
        public ICollection<Wallet> Wallets { get; set; } = [];
        public ICollection<Account> Accounts { get; set; } = [];
        public ICollection<Category> Categories { get; set; } = [];
        public ICollection<Tag> Tags { get; set; } = [];
        public ICollection<AuditLogin> AuditLogins { get; set; } = [];
        public ICollection<Record> Records { get; set; } = [];
        public ICollection<RecordTemplate> RecordTemplates { get; set; } = [];
        public ICollection<PeriodicRecordDefinition> PeriodicRecordDefinitions { get; set; } = [];
        public ICollection<RecordTagRelation> RecordTagRelations { get; set; } = [];
        public ICollection<WalletCategoryRelation> WalletCategoryRelations { get; set; } = [];
        public ICollection<WalletTagRelation> WalletTagRelations { get; set; } = [];
    }
}
