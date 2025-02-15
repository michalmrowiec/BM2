using BM2.Application.DTOs;

namespace BM2.Application.Functions.Account.Queries.Requests;

public record GetAccountsForWalletByIdQuery(Guid WalletId, Guid UserId) : IBaseRequestCollection<AccountDTO>;