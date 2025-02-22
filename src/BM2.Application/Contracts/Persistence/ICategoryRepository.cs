using BM2.Application.Contracts.Persistence.Base;
using BM2.Domain.Entities.UserProfile;

namespace BM2.Application.Contracts.Persistence;

public interface ICategoryRepository : IGenericRepository<Category>
{
    /// <summary>
    /// Retrieves a list of categories associated with a specific wallet belonging to a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user who owns the wallet.</param>
    /// <param name="walletId">The unique identifier of the wallet for which categories should be retrieved.</param>
    /// <param name="isActive">
    /// A flag indicating whether to fetch only active, inactive, or all categories.
    /// Possible values:
    /// <list type="bullet">
    /// <item><description><c>true</c> – retrieves only active categories.</description></item>
    /// <item><description><c>false</c> – retrieves only inactive categories.</description></item>
    /// <item><description><c>null</c> – retrieves all categories, regardless of their status.</description></item>
    /// </list>
    /// </param>
    /// <returns>A read-only list of categories that match the filtering criteria.</returns>
    Task<IReadOnlyList<Category>> GetCategoryForWalletAsync (Guid userId, Guid walletId, bool? isActive = null);
}