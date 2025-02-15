using BM2.Application.Contracts.Persistence.Base;
using BM2.Domain.Entities.UserProfile;

namespace BM2.Application.Contracts.Persistence;

public interface IWalletCategoryRelationRepository : IGenericRepository<WalletCategoryRelation>
{
    Task<WalletCategoryRelation?> GetRelationForWalletAsync (Guid userId, Guid walletId, Guid categoryId) ;
    Task<WalletCategoryRelation?> GetRelationForAccountAsync (Guid userId, Guid accountId, Guid categoryId);
}