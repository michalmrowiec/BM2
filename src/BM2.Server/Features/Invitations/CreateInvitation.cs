using BM2.Contracts.Invitations;
using BM2.Domain.Access;
using BM2.Domain.Common;
using BM2.Domain.Invitations;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Invitations;

internal static class CreateInvitation
{
    public static void Map(RouteGroupBuilder wallet) =>
        wallet.MapPost("/invitations", HandleAsync).Validate<CreateInvitationRequest>();

    private static async Task<Results<Created<CreatedInvitation>, ProblemHttpResult>> HandleAsync(
        CreateInvitationRequest request, CurrentWallet current, ICurrentUser currentUser, AppDbContext db, TimeProvider time,
        CancellationToken ct)
    {
        if (!current.Access.Can(WalletPermission.ManageMembers))
        {
            return Error.Forbidden().ToProblem();
        }

        if (request.AccountId is { } accountId && !await db.Accounts.AnyAsync(a => a.Id == accountId && a.WalletId == current.Id, ct))
        {
            return Error.NotFound().ToProblem();
        }

        var (invitation, token) = Invitation.Create(current.Id, request.AccountId, request.Role, currentUser.UserId, time.GetUtcNow());
        db.Invitations.Add(invitation);
        await db.SaveChangesAsync(ct);

        return TypedResults.Created(
            $"{ApiPaths.Prefix}/invitations/{token}", new CreatedInvitation(invitation.Id, token, invitation.ExpiresAt));
    }
}
