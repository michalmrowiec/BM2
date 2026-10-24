using BM2.Shared.DTOs.Interfaces;

namespace BM2.Shared.DTOs;

public abstract class BaseRecordDTO : IEntityDTO
{
    public Guid Id { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid StatusId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public decimal AccountAmount { get; set; } // Amount in Account default currency
    public decimal? PlannedAmount { get; set; }
    public Guid CurrencyId { get; set; }
    
    public CurrencyDTO? Currency { get; set; }
    public CategoryDTO? Category { get; set; }
    public RecordStatusDTO? Status { get; set; }
    public ICollection<TagDTO> Tags { get; set; } = [];

    public BaseRecordDTO()
    { }

    public BaseRecordDTO(BaseRecordDTO baseRecord)
    {
        Id = baseRecord.Id;
        CategoryId = baseRecord.CategoryId;
        StatusId = baseRecord.StatusId;
        Name = baseRecord.Name;
        Description = baseRecord.Description;
        Amount = baseRecord.Amount;
        AccountAmount = baseRecord.AccountAmount;
        PlannedAmount = baseRecord.PlannedAmount;
        CurrencyId = baseRecord.CurrencyId;
        Currency = baseRecord.Currency;
    }
}