using System.Linq.Expressions;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Domain.Entities.UserProfile;

namespace BM2.Application.Contracts.Persistence;

public interface IAccountRepository : IGenericRepository<Account>
{
    Task<IReadOnlyList<Account>> GetAllAccountsForWalletAsync(Guid walletId, params Expression<Func<Account, object>>[] includes);
}