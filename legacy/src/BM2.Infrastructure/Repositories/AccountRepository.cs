using BM2.Domain.Entities.UserProfile;
using BM2.Infrastructure.Repositories.Base;

namespace BM2.Infrastructure.Repositories;

public class AccountRepository(
    BM2DbContext context) : GenericRepository<Account>(context)
{
    public async Task<IReadOnlyList<Account>> GetActiveForUserAsync(Guid userId, params Func<IQueryable<Account>, IQueryable<Account>>[] includes)
        => await GetListByAsync(x => x.OwnedByUserId == userId && (x.IsActive), includes);
}