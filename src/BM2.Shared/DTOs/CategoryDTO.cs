using BM2.Shared.DTOs.Interfaces;

namespace BM2.Shared.DTOs;

public class CategoryDTO : IEntityDTO
{
    public Guid Id { get; set; }
    public string CategoryName { get; set; }
    
    public override string ToString()
    {
        return CategoryName;
    }
}

public class CategoryWalletRelationDTO : CategoryDTO
{
    public IList<WalletRelationDTO> WalletRelations { get; set; }
}