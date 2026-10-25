using BM2.Domain.Records;
using BM2.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Records;

public static class RecordContextLoader
{
    /// <summary>Loads what the draft points to by id, from any wallet: the domain decides what is out of bounds.</summary>
    public static async Task<RecordContext> LoadAsync(AppDbContext db, Guid walletId, RecordDraft draft, CancellationToken ct)
    {
        List<Guid> accountIds = draft.TransferAccountId is { } target ? [draft.AccountId, target] : [draft.AccountId];
        var accounts = await db.Accounts
            .AsNoTracking()
            .Where(a => accountIds.Contains(a.Id))
            .Select(a => new AccountRef(a.Id, a.WalletId, a.Currency, a.IsArchived))
            .ToListAsync(ct);

        var category = draft.CategoryId is { } categoryId
            ? await db.Categories
                .AsNoTracking()
                .Where(c => c.Id == categoryId)
                .Select(c => new CategoryRef(c.Id, c.WalletId, c.Type, c.IsArchived))
                .SingleOrDefaultAsync(ct)
            : null;

        var tagIds = draft.TagIds;
        var tags = tagIds.Count == 0
            ? []
            : await db.Tags.AsNoTracking().Where(t => tagIds.Contains(t.Id)).Select(t => new TagRef(t.Id, t.WalletId)).ToListAsync(ct);

        return new RecordContext(
            walletId,
            accounts.Find(a => a.Id == draft.AccountId),
            draft.TransferAccountId is { } transferId ? accounts.Find(a => a.Id == transferId) : null,
            category,
            tags);
    }
}
