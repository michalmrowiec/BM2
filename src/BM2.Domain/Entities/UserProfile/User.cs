namespace BM2.Domain.Entities
{
    public class User : IEntity, IEntityAudit
    {
        public Guid Id { get; set; }
        public string EmailAddress { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public bool IsActive { get; set; }
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
    }
}
