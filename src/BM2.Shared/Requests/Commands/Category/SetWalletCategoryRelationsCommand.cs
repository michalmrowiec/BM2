using System.Text.Json.Serialization;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.CommonCommands;

namespace BM2.Shared.Requests.Commands.Category;

public class SetWalletCategoryRelationsCommand : IBaseRequestCollection<CategoryWalletRelationDTO>
{
    public IList<CategoryWalletRelationCommand> CategoryWalletRelations { get; set; } = [];
    [JsonIgnore]
    public Guid UserId { get; set; }
}

public class CategoryWalletRelationCommand : WalletRelationCommand
{
    public Guid CategoryId { get; set; }
    //public IList<WalletRelationCommand>? WalletRelations { get; set; }
}