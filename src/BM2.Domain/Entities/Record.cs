namespace BM2.Domain.Entities;

public class Record
{
    public Guid RecordId { get; set; }
    public Guid WalletId { get; set; }
    public Guid CategoryId { get; set; }
    public Guid StatusId { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public decimal PlannedAmount { get; set; }
    public Guid CurrencyId { get; set; }
    public DateTime RecordDateTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    public Currency Currency { get; set; }
    public Wallet Wallet { get; set; }
    public Category Category { get; set; }
    public List<RecordTagRelation> RecordTagRelations { get; set; } = [];
}