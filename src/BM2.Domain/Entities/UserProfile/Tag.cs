using BM2.Domain.Entities.Interfaces;
using BM2.Domain.Entities.UserRecords;

namespace BM2.Domain.Entities.UserProfile;

public class Tag : IEntity, IEntityAudit, IOwnedByUser
{
    public Guid Id { get; set; }
    //public Guid WalletId { get; set; }
    public string TagName { get; set; } = null!;
    public Guid OwnedByUserId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public User? OwnedByUser { get; set; }
    public ICollection<Wallet> Wallets { get; set; } = [];
    public ICollection<BaseRecord> Records { get; set; } = [];
    
    public static int TagNameMaxLength => 150;
}