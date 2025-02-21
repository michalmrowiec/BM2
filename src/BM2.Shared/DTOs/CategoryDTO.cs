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

public enum RelationStatus
{
    Active,
    Inactive,
    NotExist
}

// TODO
// public class CategoryWithWalletRelationDTO2
// {
//     public List<WalletBasicDTO> Wallets { get; set; } = new();
//     public List<WalletCategoryRelationDTO2> CategoWalletCategoryRelationDTO { get; set; } = new();
// }
//
// public class WalletCategoryRelationDTO2
// {
//     public CategoryDTO Category { get; set; }
//     public List<WalletRelationDTO> WalletRelations  { get; set; } = new();
// }
//
// public class WalletRelationDTO
// {
//     public Guid WalletId { get; set; }
//     public bool RelationExists { get; set; }
// }