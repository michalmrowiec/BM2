namespace BM2.Shared.DTOs;

public class CategoryDTO
{
    public Guid Id { get; set; }
    public string CategoryName { get; set; }
}

public class CategoryWithWalletRelationDTO : CategoryDTO
{
    public IList<WalletCategoryRelationDTO> WalletRelations { get; set; }
}

public class WalletCategoryRelationDTO
{
    public Guid WalletId { get; set; }
    public RelationStatus Status { get; set; }
}

// TODO: Move this to the correct location
public enum RelationStatus
{
    Inactive = 0,
    Active = 1,
    NotExist = 2
}