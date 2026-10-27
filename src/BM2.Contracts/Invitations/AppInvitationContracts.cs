using FluentValidation;

namespace BM2.Contracts.Invitations;

public enum AppInvitationStatus
{
    Active,
    Used,
    Expired,
    Revoked,
}

/// <param name="UsedBy">Email of the account registered with the link, once used.</param>
public sealed record AppInvitationItem(
    Guid Id, string? Email, AppInvitationStatus Status, DateTimeOffset CreatedAt, DateTimeOffset ExpiresAt, string? UsedBy);

/// <param name="Email">Optional: only this address can register with the link.</param>
public sealed record CreateAppInvitationRequest(string? Email);

public sealed class CreateAppInvitationRequestValidator : AbstractValidator<CreateAppInvitationRequest>
{
    public const int MaxEmail = 256;

    public CreateAppInvitationRequestValidator() =>
        RuleFor(x => x.Email)
            .MaximumLength(MaxEmail).WithMessage("Email is too long.")
            .EmailAddress().WithMessage("Enter a valid email address.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
}
