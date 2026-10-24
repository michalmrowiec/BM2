using BM2.Contracts.Wallets;
using BM2.Domain.Common;

namespace BM2.Domain.Invitations;

/// <summary>A one-time link to join a wallet, or one of its accounts when <see cref="AccountId"/> is set.</summary>
public sealed class Invitation
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromDays(7);

    private Invitation()
    {
        TokenHash = [];
    }

    public Guid Id { get; private set; }

    public Guid WalletId { get; private set; }

    public Guid? AccountId { get; private set; }

    public MemberRole Role { get; private set; }

    public byte[] TokenHash { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public Guid? AcceptedByUserId { get; private set; }

    public DateTimeOffset? AcceptedAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    /// <returns>The invitation and its token; the token is not kept anywhere else.</returns>
    public static (Invitation Invitation, string Token) Create(
        Guid walletId, Guid? accountId, MemberRole role, Guid createdBy, DateTimeOffset now)
    {
        var (token, hash) = InvitationToken.Generate();
        var invitation = new Invitation
        {
            WalletId = walletId,
            AccountId = accountId,
            Role = role,
            TokenHash = hash,
            CreatedByUserId = createdBy,
            CreatedAt = now,
            ExpiresAt = now + Lifetime,
        };
        return (invitation, token);
    }

    public bool IsPending(DateTimeOffset now) => CheckUsable(now).IsSuccess;

    public Result CheckUsable(DateTimeOffset now)
    {
        if (AcceptedAt is not null)
        {
            return Error.Gone("This invitation was already used.");
        }

        if (RevokedAt is not null)
        {
            return Error.Gone("This invitation was revoked.");
        }

        return now >= ExpiresAt ? Error.Gone("This invitation has expired.") : Result.Success;
    }

    public Result Accept(Guid userId, DateTimeOffset now)
    {
        var usable = CheckUsable(now);
        if (!usable.IsSuccess)
        {
            return usable;
        }

        AcceptedByUserId = userId;
        AcceptedAt = now;
        return Result.Success;
    }

    public void Revoke(DateTimeOffset now) => RevokedAt ??= now;
}
