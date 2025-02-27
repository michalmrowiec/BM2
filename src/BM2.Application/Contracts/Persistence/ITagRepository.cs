using BM2.Application.Contracts.Persistence.Base;
using BM2.Domain.Entities.UserProfile;

namespace BM2.Application.Contracts.Persistence;

public interface ITagRepository : IGenericRepository<Tag>
{
    /// <summary>
    /// Retrieves a list of tags associated with a specific wallet belonging to a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user who owns the wallet.</param>
    /// <param name="walletId">The unique identifier of the wallet for which tags should be retrieved.</param>
    /// <param name="isActive">
    /// A flag indicating whether to fetch only active, inactive, or all tags.
    /// Possible values:
    /// <list type="bullet">
    /// <item><description><c>true</c> – retrieves only active tags.</description></item>
    /// <item><description><c>false</c> – retrieves only inactive tags.</description></item>
    /// <item><description><c>null</c> – retrieves all tags, regardless of their status.</description></item>
    /// </list>
    /// </param>
    /// <returns>A read-only list of tags that match the filtering criteria.</returns>
    Task<IReadOnlyList<Tag>> GetTagsForWalletAsync (Guid userId, Guid walletId, bool? isActive = null);
}