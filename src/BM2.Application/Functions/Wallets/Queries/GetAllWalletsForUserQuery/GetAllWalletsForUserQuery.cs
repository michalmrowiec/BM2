using BM2.Application.DTOs;

namespace BM2.Application.Functions.Wallets.Queries.GetAllWalletsForUserQuery;

public record GetAllWalletsForUserQuery(Guid UserId) : IBaseRequestCollection<WalletDTO>;