namespace BM2.Domain.Entities;

public class UserWalletRelation : IEntity, IOwnedByUser
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid WalletId { get; set; }
    public Guid OwnedByUserId { get; set; }
    
    public User User { get; set; }
    public Wallet Wallet { get; set; }
}