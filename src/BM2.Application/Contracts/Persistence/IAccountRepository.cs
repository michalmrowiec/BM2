using System.Linq.Expressions;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Domain.Entities.UserProfile;

namespace BM2.Application.Contracts.Persistence;

public interface IAccountRepository : IGenericRepository<Account>
{
    Task<IReadOnlyList<Account>> GetAllAccountsForWalletAsync(Guid walletId, params Func<IQueryable<Account>, IQueryable<Account>>[] includes);
    Task<IReadOnlyList<Account>> GetAllForUserAsync(Guid userId, bool activeOnly = false, params Func<IQueryable<Account>, IQueryable<Account>>[] includes);

}