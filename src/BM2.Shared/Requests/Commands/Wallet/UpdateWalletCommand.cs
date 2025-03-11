using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.Record;

namespace BM2.Shared.Requests.Commands.Wallet;

public class UpdateWalletCommand : AddWalletCommand, IBaseRequest<WalletDTO>
{
    public Guid Id { get; set; }
}