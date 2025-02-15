using BM2.Application.DTOs;

namespace BM2.Application.Functions.Wallets.Queries.Requests;

public record GetAllWalletsForUserQuery(Guid UserId) : IBaseRequestCollection<WalletDTO>;