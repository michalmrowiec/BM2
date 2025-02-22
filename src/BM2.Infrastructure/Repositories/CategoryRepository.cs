using BM2.Application.Contracts.Persistence;
using BM2.Domain.Entities.UserProfile;
using BM2.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace BM2.Infrastructure.Repositories;

public class CategoryRepository(
    BM2DbContext context) : GenericRepository<Category>(context), ICategoryRepository
{
    private readonly BM2DbContext _context = context;

    public async Task<IReadOnlyList<Category>> GetCategoryForWalletAsync(Guid userId, Guid walletId,
        bool? isActive = null)
    {
        var relations = _context.WalletCategoryRelations.Where(x =>
            x.OwnedByUserId == userId && x.WalletId == walletId);

        relations = isActive switch
        {
            true => relations.Where(x => x.IsActive),
            false => relations.Where(x => !x.IsActive),
            _ => relations
        };

        var categoryIds = relations.Select(x => x.CategoryId).Distinct();

        var categories = await _context.Categories.Where(x => categoryIds.Contains(x.Id)).ToListAsync();

        return categories;
    }
}