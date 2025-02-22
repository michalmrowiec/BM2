using BM2.Application.Contracts.Persistence;
using BM2.Domain.Entities.UserProfile;
using BM2.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace BM2.Infrastructure.Repositories;

public class CategoryRepository(
    BM2DbContext context) : GenericRepository<Category>(context), ICategoryRepository
{
    private readonly BM2DbContext _context = context;

    public async Task<IReadOnlyList<Category>> GetCategoryForWalletAsync(Guid userId, Guid walletId)
    {
        var categoriesIds = _context.WalletCategoryRelations.Where(x =>
            x.OwnedByUserId == userId && x.WalletId == walletId).Select(x => x.CategoryId);

        var categories = await _context.Categories.Where(x => categoriesIds.Contains(x.Id)).ToListAsync();

        return categories;
    }
}