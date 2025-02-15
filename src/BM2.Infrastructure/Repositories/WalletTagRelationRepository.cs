using BM2.Application.Contracts.Persistence;
using BM2.Domain.Entities.UserProfile;
using BM2.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace BM2.Infrastructure.Repositories;

public class WalletTagRelationRepository(
    BM2DbContext context) : GenericRepository<WalletTagRelation>(context), IWalletTagRelationRepository
{
    public async Task<IReadOnlyList<WalletTagRelation>> GetRelationForAccountAsync(Guid userId, Guid accountId, params IList<Guid> tagIds) =>
        await GetListByAsync(relation =>
            relation.OwnedByUserId == userId
            && tagIds.Contains(relation.TagId)
            && relation.Wallet != null
            && relation.Wallet.Accounts.Any(account => account.Id == accountId),
            q => q.Include(r => r.Wallet).ThenInclude(w => w!.Accounts)
        );
}