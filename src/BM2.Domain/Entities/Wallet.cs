namespace BM2.Domain.Entities
{
    public class Wallet
    {
        public Guid WalletId { get; set; }
        public string WalletName { get; set; }
        public bool IsActive { get; set; }
        public Guid DefaultCurrencyId { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }

        public Currency DefaultCurrency { get; set; }
        public List<Record> Records { get; set; } = [];
        public List<User> Users { get; set; } = [];
        public List<WalletCategoryRelation> WalletCategories { get; set; } = [];
        public List<WalletTagRelation> WalletTagRelations { get; set; } = [];
    }
}
