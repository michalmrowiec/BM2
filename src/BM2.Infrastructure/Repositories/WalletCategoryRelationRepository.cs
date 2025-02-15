using BM2.Application.Contracts.Persistence;
using BM2.Domain.Entities.UserProfile;
using BM2.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace BM2.Infrastructure.Repositories;

public class WalletCategoryRelationRepository(
    BM2DbContext context) : GenericRepository<WalletCategoryRelation>(context), IWalletCategoryRelationRepository
{
    public async Task<WalletCategoryRelation?> GetRelationForWalletAsync(Guid userId, Guid walletId, Guid categoryId) =>
        await GetByAsync(x =>
            x.OwnedByUserId == userId
            && x.CategoryId == categoryId
            && x.WalletId == walletId);

    public async Task<WalletCategoryRelation?>
        GetRelationForAccountAsync(Guid userId, Guid accountId, Guid categoryId) =>
        await GetByAsync(relation =>
                relation.OwnedByUserId == userId
                && relation.CategoryId == categoryId
                && relation.Wallet != null
                && relation.Wallet.Accounts.Any(account => account.Id == accountId),
            q => q.Include(r => r.Wallet).ThenInclude(w => w!.Accounts)
        );
}