using BM2.Domain.Entities.UserProfile;
using BM2.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace BM2.Infrastructure.Repositories;

public class WalletTagRelationRepository(
    BM2DbContext context) : GenericRepository<WalletTagRelation>(context)
{
    public async Task<IReadOnlyList<WalletTagRelation>> GetRelationForAccountAsync(Guid userId, Guid accountId, params IList<Guid> tagIds) =>
        await GetListByAsync(relation =>
            relation.OwnedByUserId == userId
            && tagIds.Contains(relation.TagId)
            && relation.Wallet != null
            && relation.Wallet.Accounts.Any(account => account.Id == accountId),
            q => q.Include(r => r.Wallet).ThenInclude(w => w!.Accounts)
        );
    
    public async Task<IReadOnlyList<WalletTagRelation>> GetRelationForWalletAsync(Guid userId, Guid walletId, params IList<Guid> tagIds) =>
        await GetListByAsync(relation =>
                relation.OwnedByUserId == userId
                && tagIds.Contains(relation.TagId)
                && relation.Wallet != null
                && relation.Wallet.Id == walletId,
            q => q.Include(r => r.Wallet)
        );
}