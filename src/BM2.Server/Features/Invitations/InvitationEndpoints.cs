using BM2.Domain.Invitations;
using BM2.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Invitations;

public static class InvitationEndpoints
{
    /// <summary>Creating and revoking, inside the wallet group (owners only).</summary>
    public static RouteGroupBuilder MapInvitationManagementEndpoints(this RouteGroupBuilder wallet)
    {
        CreateInvitation.Map(wallet);
        RevokeInvitation.Map(wallet);
        return wallet;
    }

    /// <summary>Preview and accept by token, for any signed-in user.</summary>
    public static RouteGroupBuilder MapInvitationEndpoints(this RouteGroupBuilder api)
    {
        var invitation = api.MapGroup("/invitations/{token}");
        GetInvitation.Map(invitation);
        AcceptInvitation.Map(invitation);
        return api;
    }
}

public static class InvitationLookup
{
    /// <summary>The invitation for a link token; null for unknown or malformed tokens.</summary>
    public static async Task<Invitation?> FindAsync(AppDbContext db, string token, CancellationToken ct) =>
        InvitationToken.TryHash(token, out var hash)
            ? await db.Invitations.SingleOrDefaultAsync(i => i.TokenHash == hash, ct)
            : null;
}
