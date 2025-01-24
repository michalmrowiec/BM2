using Microsoft.VisualBasic;

namespace BM2.Domain.Entities
{
    public class Wallet : IEntity, IEntityAudit, IOwnedByUser
    {
        public Guid Id { get; set; }
        public string WalletName { get; set; }
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
        public ICollection<Account> Accounts { get; set; }
        public ICollection<Category> Categories { get; set; } = [];
        public ICollection<Tag> Tags { get; set; } = [];
    }
}
