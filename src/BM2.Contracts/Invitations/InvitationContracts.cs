using BM2.Contracts.Wallets;
using FluentValidation;

namespace BM2.Contracts.Invitations;

/// <param name="AccountId">Set when the invitation shares one account instead of the whole wallet.</param>
public sealed record InvitationItem(Guid Id, MemberRole Role, Guid? AccountId, string? AccountName, DateTimeOffset ExpiresAt);

public sealed record CreateInvitationRequest(MemberRole Role, Guid? AccountId);

/// <param name="Token">Shown once; the link is built from it on the client.</param>
public sealed record CreatedInvitation(Guid Id, string Token, DateTimeOffset ExpiresAt);

public sealed record InvitationPreview(Guid WalletId, string WalletName, string InvitedBy, MemberRole Role, string? AccountName);

public sealed record AcceptedInvitation(Guid WalletId);

public sealed class CreateInvitationRequestValidator : AbstractValidator<CreateInvitationRequest>
{
    public CreateInvitationRequestValidator() =>
        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Choose a role.")
            .NotEqual(MemberRole.Owner).When(x => x.AccountId is not null)
            .WithMessage("An account can be shared only with the editor or viewer role.");
}
