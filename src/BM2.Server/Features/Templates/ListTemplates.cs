using BM2.Contracts.Templates;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Templates;

internal static class ListTemplates
{
    public static void Map(RouteGroupBuilder templates) => templates.MapGet("/", HandleAsync);

    private static async Task<Ok<List<TemplateItem>>> HandleAsync(CurrentWallet current, AppDbContext db, CancellationToken ct) =>
        TypedResults.Ok(await LoadAsync(current, db, ct));

    public static async Task<List<TemplateItem>> LoadAsync(CurrentWallet current, AppDbContext db, CancellationToken ct)
    {
        var access = current.Access;
        var walletId = current.Id;
        var templates = await db.Templates
            .AsNoTracking()
            .Where(t => t.WalletId == walletId)
            .OrderByDescending(t => t.IsPinned).ThenByDescending(t => t.CreatedAt)
            .Select(t => new
            {
                t.Id, t.Type, t.AccountId, t.CategoryId, t.TransferAccountId, t.Amount, t.Currency, t.Name, t.IsPinned,
                TagIds = t.Tags.Select(x => x.TagId).ToList(),
            })
            .ToListAsync(ct);

        // Archived, deleted or unreadable items read as empty, so the form falls back to its defaults.
        var accounts = (await db.Accounts.Where(a => a.WalletId == walletId && !a.IsArchived).Select(a => a.Id).ToListAsync(ct))
            .Where(access.CanReadAccount).ToHashSet();
        var categories = (await db.Categories.Where(c => c.WalletId == walletId && !c.IsArchived).Select(c => c.Id).ToListAsync(ct))
            .ToHashSet();
        var tags = (await db.Tags.Where(t => t.WalletId == walletId && !t.IsArchived).Select(t => t.Id).ToListAsync(ct)).ToHashSet();

        return templates
            .Select(t => new TemplateItem(
                t.Id,
                t.Type,
                Keep(t.AccountId, accounts),
                Keep(t.CategoryId, categories),
                Keep(t.TransferAccountId, accounts),
                t.Amount,
                t.Currency,
                t.Name,
                t.TagIds.Where(tags.Contains).ToList(),
                t.IsPinned))
            .ToList();
    }

    private static Guid? Keep(Guid? id, HashSet<Guid> active) => id is { } value && active.Contains(value) ? value : null;
}
