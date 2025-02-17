using BM2.Shared.DTOs;

namespace BM2.Shared.Requests.Queries.Account;

public record GetAccountsForWalletByIdQuery(Guid WalletId, Guid UserId) : IBaseRequestCollection<AccountDTO>;