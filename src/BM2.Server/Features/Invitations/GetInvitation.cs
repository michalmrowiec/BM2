using BM2.Contracts.Invitations;
using BM2.Domain.Common;
using BM2.Server.Data;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Invitations;

internal static class GetInvitation
{
    public static void Map(RouteGroupBuilder invitation) => invitation.MapGet("/", HandleAsync);

    private static async Task<Results<Ok<InvitationPreview>, ProblemHttpResult>> HandleAsync(
        string token, AppDbContext db, TimeProvider time, CancellationToken ct)
    {
        var invitation = await InvitationLookup.FindAsync(db, token, ct);
        if (invitation is null)
        {
            return Error.NotFound().ToProblem();
        }

        var usable = invitation.CheckUsable(time.GetUtcNow());
        if (!usable.IsSuccess)
        {
            return usable.Error!.ToProblem();
        }

        var walletName = await db.Wallets.Where(w => w.Id == invitation.WalletId).Select(w => w.Name).SingleAsync(ct);
        var invitedBy = await db.Users.Where(u => u.Id == invitation.CreatedByUserId).Select(u => u.Email).SingleOrDefaultAsync(ct);
        var accountName = invitation.AccountId is { } accountId
            ? await db.Accounts.Where(a => a.Id == accountId).Select(a => a.Name).SingleOrDefaultAsync(ct)
            : null;

        return TypedResults.Ok(new InvitationPreview(
            invitation.WalletId, walletName, invitedBy ?? "A former member", invitation.Role, accountName));
    }
}
