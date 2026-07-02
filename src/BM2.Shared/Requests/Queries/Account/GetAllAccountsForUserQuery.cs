using BM2.Shared.DTOs;

namespace BM2.Shared.Requests.Queries.Account;

public record GetAllAccountsForUserQuery(Guid UserId, bool ActiveOnly = false) : IBaseRequestCollection<AccountDTO>;