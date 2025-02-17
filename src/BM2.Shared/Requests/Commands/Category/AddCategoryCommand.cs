using System.Text.Json.Serialization;
using BM2.Shared.DTOs;

namespace BM2.Shared.Requests.Commands.Category;

public class AddCategoryCommand : IBaseRequest<CategoryDTO>
{
    public string CategoryName { get; set; } = null!;
    public List<Guid> WalletIds  { get; set; } = [];
    [JsonIgnore]
    public Guid OwnedByUserId { get; set; }
}