using BM2.Application.Functions.DTOs;
using BM2.Application.Responses;
using BM2.Domain.Entities;
using MediatR;

namespace BM2.Application.Functions.Requests.Command;

public class AddWalletCommand : IRequest<BaseResponse<WalletDTO>>
{
    public string WalletName { get; set; } = null!;
    public bool IsActive { get; set; }
    public Guid DefaultCurrencyId { get; set; }
    public Guid OwnedByUserId { get; set; }
}