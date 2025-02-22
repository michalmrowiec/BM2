using BM2.Shared.Models;

namespace BM2.Shared.DTOs;

public class WalletRelationDTO
{
    public Guid WalletId { get; set; }
    public RelationStatus Status { get; set; }
}