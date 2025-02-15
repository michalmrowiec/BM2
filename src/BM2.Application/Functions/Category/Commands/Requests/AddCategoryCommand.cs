using System.Text.Json.Serialization;
using BM2.Application.DTOs;

namespace BM2.Application.Functions.Category.Commands.Requests;

public class AddCategoryCommand : IBaseRequest<CategoryDTO>
{
    public string CategoryName { get; set; } = null!;
    public List<Guid> WalletIds  { get; set; } = [];
    [JsonIgnore]
    public Guid OwnedByUserId { get; set; }
}