using BM2.Contracts.Invitations;
using BM2.Domain.Common;
using BM2.Server.Data;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BM2.Server.Features.Invitations;

internal static class AcceptInvitation
{
    public static void Map(RouteGroupBuilder invitation) => invitation.MapPost("/accept", HandleAsync);

    private static async Task<Results<Ok<AcceptedInvitation>, ProblemHttpResult>> HandleAsync(
        string token, ICurrentUser currentUser, AppDbContext db, TimeProvider time, CancellationToken ct)
    {
        var invitation = await InvitationLookup.FindAsync(db, token, ct);
        if (invitation is null)
        {
            return Error.NotFound().ToProblem();
        }

        var result = await InvitationAcceptance.AcceptAsync(db, invitation, currentUser.UserId, time.GetUtcNow(), ct);
        if (!result.IsSuccess)
        {
            return result.Error!.ToProblem();
        }

        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new AcceptedInvitation(invitation.WalletId));
    }
}
