using BM2.Shared.Models;

namespace BM2.Shared.Requests.Commands.CommonCommands;

public class WalletRelationCommand
{
    public Guid WalletId { get; set; }
    public RelationStatus Status { get; set; }
}