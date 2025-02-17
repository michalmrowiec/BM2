using System.Text.Json.Serialization;
using BM2.Shared.DTOs;

namespace BM2.Shared.Requests.Commands.Tag;

public class AddTagCommand : IBaseRequest<TagDTO>
{
    public string TagName { get; set; } = null!;
    public List<Guid> WalletIds  { get; set; } = [];
    [JsonIgnore]
    public Guid OwnedByUserId { get; set; }
}