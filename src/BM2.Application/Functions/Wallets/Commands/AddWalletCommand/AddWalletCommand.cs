using System.Text.Json.Serialization;
using BM2.Application.DTOs;
using BM2.Application.Responses;
using MediatR;

namespace BM2.Application.Functions.Wallets.Commands.AddWalletCommand;

public class AddWalletCommand : IRequest<BaseResponse<WalletDTO>>
{
    public string WalletName { get; set; } = null!;
    public bool IsActive { get; set; }
    public Guid DefaultCurrencyId { get; set; }
    [JsonIgnore]
    public Guid OwnedByUserId { get; set; }
}