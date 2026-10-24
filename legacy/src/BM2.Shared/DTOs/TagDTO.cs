using BM2.Shared.DTOs.Interfaces;

namespace BM2.Shared.DTOs;

public class TagDTO : IEntityDTO
{
    public Guid Id { get; set; }
    public string TagName { get; set; } = null!;
    
    public override string ToString()
    {
        return TagName;
    }
}

public class TagWalletRelationDTO : TagDTO
{
    public IList<WalletRelationDTO> WalletRelations { get; set; }
}