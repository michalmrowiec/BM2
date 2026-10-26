using BM2.Contracts.Accounts;
using BM2.Contracts.Wallets;
using BM2.Server.Data;
using BM2.Server.Features.Accounts;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Wallets;

internal static class ListWallets
{
    public static void Map(RouteGroupBuilder wallets) => wallets.MapGet("/", HandleAsync);

    private static async Task<Ok<List<WalletListItem>>> HandleAsync(
        ICurrentUser currentUser, AppDbContext db, CancellationToken ct)
    {
        var userId = currentUser.UserId;
        var wallets = await db.Wallets
            .AsNoTracking()
            .Where(w => w.Members.Any(m => m.UserId == userId)
                || db.Accounts.Any(a => a.WalletId == w.Id && a.Members.Any(m => m.UserId == userId)))
            .OrderBy(w => w.Name)
            .Select(w => new WalletListItem(
                w.Id,
                w.Name,
                w.BaseCurrency,
                w.Members.Where(m => m.UserId == userId).Select(m => (MemberRole?)m.Role).FirstOrDefault()))
            .ToListAsync(ct);

        // Same reach as WalletAccess: members read every account, others only the ones shared with them.
        var accounts = await db.Accounts
            .AsNoTracking()
            .Where(a => !a.IsArchived
                && (db.WalletMembers.Any(m => m.WalletId == a.WalletId && m.UserId == userId) || a.Members.Any(m => m.UserId == userId)))
            .Select(a => new { a.Id, a.WalletId, a.Currency, a.Kind })
            .ToListAsync(ct);
        var ids = accounts.Select(a => a.Id).ToList();
        var balances = await AccountBalances.GetAsync(db, ids, ct);
        var valuations = await AccountBalances.LatestValuationsAsync(
            db, accounts.Where(a => a.Kind == AccountKind.Investment).Select(a => a.Id).ToList(), ct);

        var totals = accounts
            .GroupBy(a => a.WalletId)
            .ToDictionary(g => g.Key, g => g
                .GroupBy(a => a.Currency)
                .Select(c => new WalletTotal(c.Key, c.Sum(a => valuations.TryGetValue(a.Id, out var v) ? v.Value : balances[a.Id].Balance)))
                .OrderBy(t => t.Currency)
                .ToList());

        return TypedResults.Ok(wallets.Select(w => w with { Totals = totals.GetValueOrDefault(w.Id) ?? [] }).ToList());
    }
}
