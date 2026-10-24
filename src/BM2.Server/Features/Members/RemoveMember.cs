using BM2.Domain.Access;
using BM2.Domain.Common;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Members;

internal static class RemoveMember
{
    public static void Map(RouteGroupBuilder wallet) => wallet.MapDelete("/members/{userId:guid}", HandleAsync);

    /// <summary>Owners remove anyone; every member may leave.</summary>
    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        Guid userId, CurrentWallet current, ICurrentUser currentUser, AppDbContext db, CancellationToken ct)
    {
        if (userId != currentUser.UserId && !current.Access.Can(WalletPermission.ManageMembers))
        {
            return Error.Forbidden().ToProblem();
        }

        var wallet = await db.Wallets.Include(w => w.Members).SingleAsync(w => w.Id == current.Id, ct);
        var result = wallet.RemoveMember(userId);
        if (!result.IsSuccess)
        {
            return result.Error!.ToProblem();
        }

        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}
