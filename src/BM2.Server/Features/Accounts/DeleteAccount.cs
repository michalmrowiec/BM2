using BM2.Domain.Access;
using BM2.Domain.Common;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Accounts;

internal static class DeleteAccount
{
    public static void Map(RouteGroupBuilder accounts) => accounts.MapDelete("/{accountId:guid}", HandleAsync);

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        Guid accountId, CurrentWallet current, AppDbContext db, CancellationToken ct)
    {
        if (!current.Access.Can(WalletPermission.ManageAccounts))
        {
            return Error.Forbidden().ToProblem();
        }

        var deleted = await db.Accounts.Where(a => a.Id == accountId && a.WalletId == current.Id).ExecuteDeleteAsync(ct);
        return deleted == 0 ? Error.NotFound().ToProblem() : TypedResults.NoContent();
    }
}
