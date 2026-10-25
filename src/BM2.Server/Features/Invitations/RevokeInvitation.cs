using BM2.Domain.Access;
using BM2.Domain.Common;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Invitations;

internal static class RevokeInvitation
{
    public static void Map(RouteGroupBuilder wallet) => wallet.MapDelete("/invitations/{invitationId:guid}", HandleAsync);

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        Guid invitationId, CurrentWallet current, AppDbContext db, TimeProvider time, CancellationToken ct)
    {
        if (!current.Access.Can(WalletPermission.ManageMembers))
        {
            return Error.Forbidden().ToProblem();
        }

        var invitation = await db.Invitations.SingleOrDefaultAsync(i => i.Id == invitationId && i.WalletId == current.Id, ct);
        if (invitation is null)
        {
            return Error.NotFound().ToProblem();
        }

        invitation.Revoke(time.GetUtcNow());
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}
