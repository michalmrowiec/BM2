using System.Text.Json.Serialization;
using BM2.Application.DTOs;

namespace BM2.Application.Functions.Wallets.Commands.Requests;

public class AddWalletCommand : IBaseRequest<WalletDTO>
{
    public string WalletName { get; set; } = null!;
    public bool IsActive { get; set; }
    public Guid DefaultCurrencyId { get; set; }
    [JsonIgnore]
    public Guid OwnedByUserId { get; set; }
}