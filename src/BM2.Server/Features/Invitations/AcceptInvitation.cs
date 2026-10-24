using BM2.Contracts.Invitations;
using BM2.Domain.Common;
using BM2.Server.Data;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Invitations;

internal static class AcceptInvitation
{
    public static void Map(RouteGroupBuilder invitation) => invitation.MapPost("/accept", HandleAsync);

    /// <summary>Grants the invited role; an existing higher role stays (granting never lowers a role).</summary>
    private static async Task<Results<Ok<AcceptedInvitation>, ProblemHttpResult>> HandleAsync(
        string token, ICurrentUser currentUser, AppDbContext db, TimeProvider time, CancellationToken ct)
    {
        var invitation = await InvitationLookup.FindAsync(db, token, ct);
        if (invitation is null)
        {
            return Error.NotFound().ToProblem();
        }

        var now = time.GetUtcNow();
        var userId = currentUser.UserId;
        var accepted = invitation.Accept(userId, now);
        if (!accepted.IsSuccess)
        {
            return accepted.Error!.ToProblem();
        }

        if (invitation.AccountId is { } accountId)
        {
            var account = await db.Accounts.Include(a => a.Members).SingleAsync(a => a.Id == accountId, ct);
            var granted = account.Grant(userId, invitation.Role, now);
            if (!granted.IsSuccess)
            {
                return granted.Error!.ToProblem();
            }
        }
        else
        {
            var wallet = await db.Wallets.Include(w => w.Members).SingleAsync(w => w.Id == invitation.WalletId, ct);
            wallet.Grant(userId, invitation.Role, now);
        }

        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new AcceptedInvitation(invitation.WalletId));
    }
}
