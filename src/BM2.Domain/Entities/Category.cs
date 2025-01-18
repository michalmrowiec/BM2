namespace BM2.Domain.Entities;

public class Category
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    
    public List<Record> Records { get; set; }
    public List<WalletCategoryRelation> WalletCategories { get; set; } = [];
}