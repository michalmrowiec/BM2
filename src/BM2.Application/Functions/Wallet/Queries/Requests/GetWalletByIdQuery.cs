using BM2.Application.DTOs;

namespace BM2.Application.Functions.Wallet.Queries.Requests;

public record GetWalletByIdQuery(Guid WalletId, Guid UserId) : IBaseRequest<WalletDTO>;