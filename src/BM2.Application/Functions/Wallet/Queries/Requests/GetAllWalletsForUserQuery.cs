using BM2.Application.DTOs;

namespace BM2.Application.Functions.Wallet.Queries.Requests;

public record GetAllWalletsForUserQuery(Guid UserId) : IBaseRequestCollection<WalletDTO>;