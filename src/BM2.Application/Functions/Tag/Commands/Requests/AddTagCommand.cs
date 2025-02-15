using System.Text.Json.Serialization;
using BM2.Application.DTOs;

namespace BM2.Application.Functions.Tag.Commands.Requests;

public class AddTagCommand : IBaseRequest<TagDTO>
{
    public string TagName { get; set; } = null!;
    public List<Guid> WalletIds  { get; set; } = [];
    [JsonIgnore]
    public Guid OwnedByUserId { get; set; }
}