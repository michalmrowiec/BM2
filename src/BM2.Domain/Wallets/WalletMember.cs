using BM2.Contracts.Wallets;

namespace BM2.Domain.Wallets;

public sealed class WalletMember
{
    private WalletMember()
    {
    }

    internal WalletMember(Guid userId, MemberRole role, DateTimeOffset joinedAt)
    {
        UserId = userId;
        Role = role;
        JoinedAt = joinedAt;
    }

    public Guid WalletId { get; private set; }

    public Guid UserId { get; private set; }

    public MemberRole Role { get; internal set; }

    public DateTimeOffset JoinedAt { get; private set; }
}
