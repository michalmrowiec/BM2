using BM2.Shared.DTOs.Interfaces;

namespace BM2.Shared.DTOs;

public abstract class BaseRecordDTO : IEntityDTO
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public Guid StatusId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public decimal? PlannedAmount { get; set; }
    public Guid CurrencyId { get; set; }
    
    public CurrencyDTO? Currency { get; set; }
    public CategoryDTO? Category { get; set; }
    public RecordStatusDTO? Status { get; set; }
    public ICollection<TagDTO> Tags { get; set; } = [];
}