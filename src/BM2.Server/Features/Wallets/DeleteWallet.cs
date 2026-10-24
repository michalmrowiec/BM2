using BM2.Domain.Access;
using BM2.Domain.Common;
using BM2.Server.Data;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Wallets;

internal static class DeleteWallet
{
    public static void Map(RouteGroupBuilder wallet) => wallet.MapDelete("/", HandleAsync);

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        CurrentWallet current, AppDbContext db, CancellationToken ct)
    {
        if (!current.Access.Can(WalletPermission.ManageWallet))
        {
            return Error.Forbidden().ToProblem();
        }

        var walletId = current.Id;

        // Children first: the schema has no cascade from wallets to accounts.
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await db.Accounts.Where(a => a.WalletId == walletId).ExecuteDeleteAsync(ct);
        await db.Wallets.Where(w => w.Id == walletId).ExecuteDeleteAsync(ct);
        await transaction.CommitAsync(ct);

        return TypedResults.NoContent();
    }
}
