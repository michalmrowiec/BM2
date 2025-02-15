using BM2.Domain.Entities.Interfaces;

namespace BM2.Domain.Entities.UserProfile;

public class WalletTagRelation : IEntity, IOwnedByUser
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public Guid TagId { get; set; }
    public Guid OwnedByUserId { get; set; }

    public Wallet? Wallet { get; set; }
    public Tag? Tag { get; set; }
    public User? OwnedByUser { get; set; }
}