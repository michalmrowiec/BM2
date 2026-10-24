using BM2.Contracts.Wallets;

namespace BM2.Contracts.Invitations;

/// <param name="AccountId">Set when the invitation shares one account instead of the whole wallet.</param>
public sealed record InvitationItem(Guid Id, MemberRole Role, Guid? AccountId, string? AccountName, DateTimeOffset ExpiresAt);
