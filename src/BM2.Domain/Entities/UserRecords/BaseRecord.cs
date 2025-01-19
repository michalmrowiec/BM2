namespace BM2.Domain.Entities;

public class BaseRecord : IEntity, IEntityAudit, IOwnedByUser
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public Guid CategoryId { get; set; }
    public Guid StatusId { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public decimal PlannedAmount { get; set; }
    public Guid CurrencyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}