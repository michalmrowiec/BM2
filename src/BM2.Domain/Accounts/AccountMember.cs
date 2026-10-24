using BM2.Contracts.Wallets;

namespace BM2.Domain.Accounts;

/// <summary>An account shared with a user outside the wallet.</summary>
public sealed class AccountMember
{
    private AccountMember()
    {
    }

    internal AccountMember(Guid userId, MemberRole role, DateTimeOffset joinedAt)
    {
        UserId = userId;
        Role = role;
        JoinedAt = joinedAt;
    }

    public Guid AccountId { get; private set; }

    public Guid UserId { get; private set; }

    public MemberRole Role { get; internal set; }

    public DateTimeOffset JoinedAt { get; private set; }
}
