using BM2.Contracts.Invitations;
using BM2.Contracts.Wallets;
using FluentValidation;

namespace BM2.Contracts.Members;

public sealed record MemberItem(Guid UserId, string Email, MemberRole Role, bool IsMe);

public sealed record AccountShareItem(Guid AccountId, string AccountName, Guid UserId, string Email, MemberRole Role, bool IsMe);

/// <param name="Invitations">Pending invitations; empty unless the user manages members.</param>
public sealed record MembersResponse(
    IReadOnlyList<MemberItem> Members,
    IReadOnlyList<AccountShareItem> AccountShares,
    IReadOnlyList<InvitationItem> Invitations);

public sealed record UpdateRoleRequest(MemberRole Role);

public sealed class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
{
    public UpdateRoleRequestValidator() => RuleFor(x => x.Role).IsInEnum().WithMessage("Choose a role.");
}
