using System.Text.Json.Serialization;
using BM2.Shared.DTOs;

namespace BM2.Shared.Requests.Commands.Category;

public class SetWalletCategoryRelationsCommand : IBaseRequestCollection<CategoryWithWalletRelationDTO>
{
    public IList<CategoryWithWalletRelationCommand>? CategoryWalletRelations { get; set; }
    [JsonIgnore]
    public Guid OwnedByUserId { get; set; }
}

public class CategoryWithWalletRelationCommand
{
    public Guid CategoryId { get; set; }
    public IList<WalletCategoryRelationCommand>? WalletRelations { get; set; }}

public class WalletCategoryRelationCommand
{
    public Guid WalletId { get; set; }
    public RelationStatus Status { get; set; }
}