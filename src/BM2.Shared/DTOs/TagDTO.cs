using BM2.Shared.DTOs.Interfaces;

namespace BM2.Shared.DTOs;

public class TagDTO : IEntityDTO
{
    public Guid Id { get; set; }
    public string TagName { get; set; } = null!;
}