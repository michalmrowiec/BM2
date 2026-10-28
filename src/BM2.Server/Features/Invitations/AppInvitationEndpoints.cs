using BM2.Contracts.Invitations;
using BM2.Domain.Common;
using BM2.Domain.Invitations;
using BM2.Server.Data;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Invitations;

/// <summary>Links to register in the app, managed by the user who created them.</summary>
public static class AppInvitationEndpoints
{
    private const int ListLimit = 50;

    public static RouteGroupBuilder MapAppInvitationEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/app-invitations");
        group.MapGet("/", ListAsync);
        group.MapPost("/", CreateAsync).Validate<CreateAppInvitationRequest>();
        group.MapDelete("/{invitationId:guid}", RevokeAsync);
        return api;
    }

    private static async Task<Ok<List<AppInvitationItem>>> ListAsync(
        ICurrentUser currentUser, AppDbContext db, TimeProvider time, CancellationToken ct)
    {
        var userId = currentUser.UserId;
        var rows = await db.AppInvitations
            .AsNoTracking()
            .Where(i => i.CreatedByUserId == userId)
            .OrderByDescending(i => i.CreatedAt)
            .Take(ListLimit)
            .Select(i => new
            {
                i.Id, i.Email, i.CreatedAt, i.ExpiresAt, i.UsedAt, i.RevokedAt,
                UsedBy = db.Users.Where(u => u.Id == i.UsedByUserId).Select(u => u.Email).FirstOrDefault(),
            })
            .ToListAsync(ct);

        var now = time.GetUtcNow();
        return TypedResults.Ok(rows.Select(r => new AppInvitationItem(
            r.Id,
            r.Email,
            r.UsedAt is not null ? AppInvitationStatus.Used
                : r.RevokedAt is not null ? AppInvitationStatus.Revoked
                : now >= r.ExpiresAt ? AppInvitationStatus.Expired
                : AppInvitationStatus.Active,
            r.CreatedAt,
            r.ExpiresAt,
            r.UsedBy)).ToList());
    }

    private static async Task<Results<Created<CreatedInvitation>, ProblemHttpResult>> CreateAsync(
        CreateAppInvitationRequest request, ICurrentUser currentUser, AppDbContext db, TimeProvider time, CancellationToken ct)
    {
        var userId = currentUser.UserId;
        var now = time.GetUtcNow();
        var active = await db.AppInvitations.CountAsync(
            i => i.CreatedByUserId == userId && i.UsedAt == null && i.RevokedAt == null && i.ExpiresAt > now, ct);
        if (active >= AppInvitation.MaxActivePerUser)
        {
            return Error.Conflict("You can have at most 10 active invitations. Revoke one first.").ToProblem();
        }

        var (invitation, token) = AppInvitation.Create(request.Email, userId, now);
        db.AppInvitations.Add(invitation);
        await db.SaveChangesAsync(ct);
        return TypedResults.Created(
            $"{ApiPaths.Prefix}/app-invitations/{invitation.Id}", new CreatedInvitation(invitation.Id, token, invitation.ExpiresAt));
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> RevokeAsync(
        Guid invitationId, ICurrentUser currentUser, AppDbContext db, TimeProvider time, CancellationToken ct)
    {
        var userId = currentUser.UserId;
        var invitation = await db.AppInvitations.SingleOrDefaultAsync(i => i.Id == invitationId && i.CreatedByUserId == userId, ct);
        if (invitation is null)
        {
            return Error.NotFound().ToProblem();
        }

        invitation.Revoke(time.GetUtcNow());
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}

public static class AppInvitationLookup
{
    /// <summary>The usable app invitation for a link token; null for unknown, malformed or spent tokens.</summary>
    public static async Task<AppInvitation?> FindUsableAsync(AppDbContext db, string token, DateTimeOffset now, CancellationToken ct)
    {
        if (!InvitationToken.TryHash(token, out var hash))
        {
            return null;
        }

        var invitation = await db.AppInvitations.SingleOrDefaultAsync(i => i.TokenHash == hash, ct);
        return invitation?.IsActive(now) == true ? invitation : null;
    }
}
