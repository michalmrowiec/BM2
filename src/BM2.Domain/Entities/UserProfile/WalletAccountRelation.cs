namespace BM2.Domain.Entities;

public class WalletAccountRelation : IEntity, IOwnedByUser
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public Guid AccountId { get; set; }
    public Guid OwnedByUserId { get; set; }

    public Wallet Wallet { get; set; }
    public Account Account { get; set; }
}