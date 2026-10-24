using BM2.Contracts.Accounts;
using BM2.Contracts.Categories;
using BM2.Contracts.Tags;
using BM2.Contracts.Wallets;
using BM2.Domain.Access;
using BM2.Server.Data;
using BM2.Server.Features.Accounts;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Wallets;

internal static class GetWallet
{
    public static void Map(RouteGroupBuilder wallet) => wallet.MapGet("/", HandleAsync);

    private static async Task<Ok<WalletSnapshot>> HandleAsync(CurrentWallet current, AppDbContext db, CancellationToken ct)
    {
        var access = current.Access;
        var wallet = await db.Wallets
            .AsNoTracking()
            .Where(w => w.Id == access.WalletId)
            .Select(w => new { w.Name, w.BaseCurrency })
            .SingleAsync(ct);

        var readable = access.ReadableAccountIds;
        var accounts = await db.Accounts
            .AsNoTracking()
            .Where(a => readable.Contains(a.Id))
            .OrderBy(a => a.IsArchived).ThenBy(a => a.Name)
            .Select(a => new { a.Id, a.Name, a.Currency, a.OpeningBalance, a.IsArchived })
            .ToListAsync(ct);
        var balances = await AccountBalances.GetAsync(db, readable, ct);
        var categories = await db.Categories
            .AsNoTracking()
            .Where(c => c.WalletId == access.WalletId)
            .OrderBy(c => c.IsArchived).ThenBy(c => c.Type).ThenBy(c => c.Name)
            .Select(c => new CategoryItem(c.Id, c.Name, c.Type, c.IsArchived, c.Emoji))
            .ToListAsync(ct);
        var tags = await db.Tags
            .AsNoTracking()
            .Where(t => t.WalletId == access.WalletId)
            .OrderBy(t => t.IsArchived).ThenBy(t => t.Name)
            .Select(t => new TagItem(t.Id, t.Name, t.IsArchived))
            .ToListAsync(ct);

        return TypedResults.Ok(new WalletSnapshot(
            access.WalletId,
            wallet.Name,
            wallet.BaseCurrency,
            access.WalletRole,
            new WalletPermissions(
                access.Can(WalletPermission.ManageWallet),
                access.Can(WalletPermission.ManageAccounts),
                access.Can(WalletPermission.ManageCategories),
                access.Can(WalletPermission.ViewMembers),
                access.Can(WalletPermission.ManageMembers)),
            accounts
                .Select(a => new AccountItem(
                    a.Id, a.Name, a.Currency, a.OpeningBalance, balances[a.Id], a.IsArchived, access.CanWriteAccount(a.Id)))
                .ToList(),
            categories,
            tags));
    }
}
