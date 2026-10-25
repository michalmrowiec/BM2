using BM2.Domain.Access;
using BM2.Domain.Common;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Members;

internal static class RemoveAccountShare
{
    public static void Map(RouteGroupBuilder wallet) =>
        wallet.MapDelete("/accounts/{accountId:guid}/members/{userId:guid}", HandleAsync);

    /// <summary>Owners remove any share; the person it was shared with may leave it.</summary>
    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        Guid accountId, Guid userId, CurrentWallet current, ICurrentUser currentUser, AppDbContext db, CancellationToken ct)
    {
        if (userId != currentUser.UserId && !current.Access.Can(WalletPermission.ManageMembers))
        {
            return Error.Forbidden().ToProblem();
        }

        var account = await db.Accounts.Include(a => a.Members)
            .SingleOrDefaultAsync(a => a.Id == accountId && a.WalletId == current.Id, ct);
        if (account is null)
        {
            return Error.NotFound().ToProblem();
        }

        var result = account.RemoveShare(userId);
        if (!result.IsSuccess)
        {
            return result.Error!.ToProblem();
        }

        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}
