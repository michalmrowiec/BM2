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
        
        public List<Wallet> Wallets { get; set; } = [];
        public List<Account> Accounts { get; set; } = [];
        public List<Category> Categories { get; set; } = [];
        public List<Tag> Tags { get; set; } = [];
        public List<AuditLogin> AuditLogins { get; set; } = [];
    }
}
