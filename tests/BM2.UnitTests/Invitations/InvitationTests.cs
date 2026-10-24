using BM2.Contracts.Wallets;
using BM2.Domain.Common;
using BM2.Domain.Invitations;

namespace BM2.UnitTests.Invitations;

public class InvitationTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 24, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid UserA = Guid.NewGuid(), UserB = Guid.NewGuid();

    private static (Invitation Invitation, string Token) Create() =>
        Invitation.Create(Guid.NewGuid(), null, MemberRole.Editor, Guid.NewGuid(), Now);

    [Fact]
    public void Token_is_stored_only_as_hash()
    {
        var (invitation, token) = Create();

        InvitationToken.TryHash(token, out var hash).ShouldBeTrue();
        invitation.TokenHash.ShouldBe(hash);
        invitation.TokenHash.Length.ShouldBe(32);
    }

    [Fact]
    public void Tokens_are_unique_and_url_safe()
    {
        var tokens = Enumerable.Range(0, 50).Select(_ => Create().Token).ToList();

        tokens.Distinct().Count().ShouldBe(50);
        tokens.ShouldAllBe(t => t.Length == 43 && t.All(c => char.IsAsciiLetterOrDigit(c) || c == '-' || c == '_'));
    }

    [Fact]
    public void Expires_after_seven_days()
    {
        var (invitation, _) = Create();

        invitation.ExpiresAt.ShouldBe(Now.AddDays(7));
        invitation.CheckUsable(Now.AddDays(7).AddSeconds(-1)).IsSuccess.ShouldBeTrue();
        invitation.CheckUsable(Now.AddDays(7).AddSeconds(1)).Error!.Kind.ShouldBe(ErrorKind.Gone);
    }

    [Fact]
    public void Can_be_accepted_only_once()
    {
        var (invitation, _) = Create();

        invitation.Accept(UserA, Now).IsSuccess.ShouldBeTrue();

        invitation.Accept(UserB, Now).Error!.Kind.ShouldBe(ErrorKind.Gone);
        invitation.AcceptedByUserId.ShouldBe(UserA);
    }

    [Fact]
    public void Revoked_invitation_is_gone()
    {
        var (invitation, _) = Create();

        invitation.Revoke(Now);

        invitation.CheckUsable(Now).Error!.Kind.ShouldBe(ErrorKind.Gone);
        invitation.IsPending(Now).ShouldBeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-base64!")]
    [InlineData("AAAA")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    public void Malformed_token_cannot_be_hashed(string token) => InvitationToken.TryHash(token, out _).ShouldBeFalse();
}
