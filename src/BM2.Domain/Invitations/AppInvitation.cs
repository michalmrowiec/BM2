using BM2.Domain.Common;

namespace BM2.Domain.Invitations;

/// <summary>A one-time link that lets someone register; it does not share any wallet or account.</summary>
public sealed class AppInvitation
{
    public const int MaxActivePerUser = 10;

    private AppInvitation()
    {
        TokenHash = [];
    }

    public Guid Id { get; private set; }

    public byte[] TokenHash { get; private set; }

    /// <summary>When set, only this address can register with the link.</summary>
    public string? Email { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public Guid? UsedByUserId { get; private set; }

    public DateTimeOffset? UsedAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    /// <returns>The invitation and its token; the token is not kept anywhere else.</returns>
    public static (AppInvitation Invitation, string Token) Create(string? email, Guid createdBy, DateTimeOffset now)
    {
        var (token, hash) = InvitationToken.Generate();
        var invitation = new AppInvitation
        {
            TokenHash = hash,
            Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
            CreatedByUserId = createdBy,
            CreatedAt = now,
            ExpiresAt = now + Invitation.Lifetime,
        };
        return (invitation, token);
    }

    public bool IsActive(DateTimeOffset now) => CheckUsable(now).IsSuccess;

    public Result CheckUsable(DateTimeOffset now)
    {
        if (UsedAt is not null)
        {
            return Error.Gone("This invitation was already used.");
        }

        if (RevokedAt is not null)
        {
            return Error.Gone("This invitation was revoked.");
        }

        return now >= ExpiresAt ? Error.Gone("This invitation has expired.") : Result.Success;
    }

    public bool AllowsEmail(string email) =>
        Email is null || string.Equals(Email, email.Trim(), StringComparison.OrdinalIgnoreCase);

    public Result Use(Guid userId, string email, DateTimeOffset now)
    {
        var usable = CheckUsable(now);
        if (!usable.IsSuccess)
        {
            return usable;
        }

        if (!AllowsEmail(email))
        {
            return Error.Validation("Email", "This invitation is for a different email address.");
        }

        UsedByUserId = userId;
        UsedAt = now;
        return Result.Success;
    }

    public void Revoke(DateTimeOffset now) => RevokedAt ??= now;
}
