namespace BM2.Domain.Entities;

public class WalletCategoryRelation : IEntity, IOwnedByUser
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public Guid CategoryId { get; set; }
    public bool IsActive  { get; set; }
    public Guid UserId { get; set; }

    public Wallet Wallet { get; set; }
    public Category Category { get; set; }
}