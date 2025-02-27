using BM2.Application.Contracts.Persistence;
using BM2.Domain.Entities.UserProfile;
using BM2.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace BM2.Infrastructure.Repositories;

public class TagRepository(
    BM2DbContext context) : GenericRepository<Tag>(context), ITagRepository
{
    private readonly BM2DbContext _context = context;
    
    public async Task<IReadOnlyList<Tag>> GetTagsForWalletAsync(Guid userId, Guid walletId, bool? isActive = null)
    {
        var relations = _context.WalletTagRelations.Where(x =>
            x.OwnedByUserId == userId && x.WalletId == walletId);

        relations = isActive switch
        {
            true => relations.Where(x => x.IsActive),
            false => relations.Where(x => !x.IsActive),
            _ => relations
        };

        var tagIds = relations.Select(x => x.TagId).Distinct();

        var tags = await _context.Tags.Where(x => tagIds.Contains(x.Id)).ToListAsync();

        return tags;    }
}