namespace BM2.Shared.DTOs;

public class CategoryDTO
{
    public Guid Id { get; set; }
    public string CategoryName { get; set; }
}

public class CategoryWalletRelationDTO : CategoryDTO
{
    public IList<WalletRelationDTO> WalletRelations { get; set; }
}