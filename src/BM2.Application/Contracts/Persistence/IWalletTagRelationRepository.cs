using BM2.Application.Contracts.Persistence.Base;
using BM2.Domain.Entities.UserProfile;

namespace BM2.Application.Contracts.Persistence;

public interface IWalletTagRelationRepository : IGenericRepository<WalletTagRelation>
{
    Task<IReadOnlyList<WalletTagRelation>> GetRelationForAccountAsync (Guid userId, Guid accountId, params IList<Guid> tagIds);
}