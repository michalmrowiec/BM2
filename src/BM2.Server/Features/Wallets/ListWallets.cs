using BM2.Contracts.Wallets;
using BM2.Server.Data;
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

        return TypedResults.Ok(wallets);
    }
}
