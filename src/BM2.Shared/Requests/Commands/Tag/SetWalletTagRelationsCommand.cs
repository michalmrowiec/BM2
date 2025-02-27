using System.Text.Json.Serialization;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.CommonCommands;

namespace BM2.Shared.Requests.Commands.Tag;

public class SetWalletTagRelationsCommand : IBaseRequestCollection<TagWalletRelationDTO>
{
    public IList<TagWalletRelationCommand> TagWalletRelations { get; set; } = [];
    [JsonIgnore]
    public Guid OwnedByUserId { get; set; }
}

public class TagWalletRelationCommand : WalletRelationCommand
{
    public Guid TagId { get; set; }
    //public IList<WalletRelationCommand>? WalletRelations { get; set; }
}