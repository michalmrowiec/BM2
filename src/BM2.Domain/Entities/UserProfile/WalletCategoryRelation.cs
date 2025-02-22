using BM2.Domain.Entities.Interfaces;

namespace BM2.Domain.Entities.UserProfile;

public class WalletCategoryRelation : IEntity, IOwnedByUser
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public Guid CategoryId { get; set; }
    public bool IsActive { get; set; }
    public Guid OwnedByUserId { get; set; }

    public Wallet? Wallet { get; set; }
    public Category? Category { get; set; }
    public User? OwnedByUser { get; set; }

    private WalletCategoryRelation(Guid walletId, Guid categoryId, Guid ownedByUserId, bool isActive = true)
    {
        Id = Guid.NewGuid();
        WalletId = walletId;
        CategoryId = categoryId;
        IsActive = isActive;
        OwnedByUserId = ownedByUserId;
    }

    public static WalletCategoryRelation CreateInstance(Guid walletId, Guid categoryId, Guid ownedByUserId,
        bool isActive = true)
    {
        return new WalletCategoryRelation(walletId, categoryId, ownedByUserId, isActive);
    }
}