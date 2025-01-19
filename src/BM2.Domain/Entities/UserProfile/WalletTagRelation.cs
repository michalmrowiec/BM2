namespace BM2.Domain.Entities;

public class WalletTagRelation : IEntity, IOwnedByUser
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public Guid TagId { get; set; }
    public bool IsActive  { get; set; }
    public Guid UserId { get; set; }

    public Wallet Wallet { get; set; }
    public Tag Tag { get; set; }
}