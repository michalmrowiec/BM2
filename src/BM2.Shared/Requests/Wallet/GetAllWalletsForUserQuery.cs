using BM2.Shared.DTOs;

namespace BM2.Shared.Requests.Wallet;

public record GetAllWalletsForUserQuery(Guid UserId) : IBaseRequestCollection<WalletDTO>;