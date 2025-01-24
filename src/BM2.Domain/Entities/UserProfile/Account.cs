namespace BM2.Domain.Entities;

public class Account : IEntity, IEntityAudit, IOwnedByUser
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public string AccountName { get; set; } = null!;
    public bool IsActive { get; set; }
    public Guid DefaultCurrencyId { get; set; }
    public Guid OwnedByUserId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    
    public Wallet? Wallet { get; set; }
    public Currency? DefaultCurrency { get; set; }
    public ICollection<Record> Records { get; set; } = [];
}