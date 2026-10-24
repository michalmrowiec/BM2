using BM2.Shared.DTOs;

namespace BM2.Shared.Requests.Wallet;

public record GetWalletByIdQuery(Guid WalletId, Guid UserId) : IBaseRequest<WalletDTO>;