using System.Text.Json.Serialization;
using BM2.Shared.DTOs;

namespace BM2.Shared.Requests.Commands.Wallet;

public class AddWalletCommand : IBaseRequest<WalletDTO>
{
    public string WalletName { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public Guid DefaultCurrencyId { get; set; }
    [JsonIgnore] public Guid OwnedByUserId { get; set; }
}