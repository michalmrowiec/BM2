using BM2.Client.Pages;
using BM2.ClientTests.Infrastructure;
using BM2.Contracts.Invitations;
using BM2.Contracts.Members;
using BM2.Contracts.Wallets;

namespace BM2.ClientTests.Pages;

public class MembersTests : BunitContext
{
    private const string Token = "TOKEN123";
    private static readonly Guid WalletId = Guid.NewGuid(), Me = Guid.NewGuid(), Partner = Guid.NewGuid();

    private readonly StubApi _api;

    public MembersTests()
    {
        AddAuthorization().SetAuthorized("me@example.com");
        _api = this.UseBm2Client();
    }

    private void UseMembers(WalletSnapshot snapshot) =>
        _api.WithWallets(snapshot).OnGet($"api/wallets/{WalletId}/members", new MembersResponse(
            [new MemberItem(Me, "me@example.com", snapshot.Role ?? MemberRole.Viewer, true),
             new MemberItem(Partner, "partner@example.com", MemberRole.Editor, false)],
            [],
            []));

    [Fact]
    public void Lists_members_and_marks_me()
    {
        UseMembers(Snapshots.Owner(WalletId));

        var cut = Render<Members>();

        cut.WaitForAssertion(() => cut.Markup.ShouldContain("partner@example.com"));
        cut.Markup.ShouldContain("(you)");
    }

    [Fact]
    public void Created_invitation_shows_link_with_token()
    {
        UseMembers(Snapshots.Owner(WalletId));
        _api.On(HttpMethod.Post, $"api/wallets/{WalletId}/invitations",
            _ => StubApi.Json(new CreatedInvitation(Guid.NewGuid(), Token, DateTimeOffset.UtcNow.AddDays(7))));
        var cut = Render<Members>();

        cut.WaitForElement("button.invite-button").Click();
        cut.Find(".sheet form").Submit();

        cut.WaitForAssertion(() => cut.Find("input[readonly]").GetAttribute("value").ShouldBe($"http://localhost/invite/{Token}"));
        _api.Bodies.ShouldContain(b => b.Request == $"POST api/wallets/{WalletId}/invitations" && b.Body!.Contains("\"role\":2"));
    }

    [Fact]
    public void Non_owner_does_not_see_invite_button()
    {
        UseMembers(Snapshots.Owner(WalletId) with
        {
            Role = MemberRole.Viewer,
            Permissions = new WalletPermissions(false, false, false, true, false),
        });

        var cut = Render<Members>();

        cut.WaitForAssertion(() => cut.Markup.ShouldContain("partner@example.com"));
        cut.FindAll("button").ShouldNotContain(b => b.TextContent.Contains("Invite"));
        cut.FindAll("button").ShouldContain(b => b.TextContent.Contains("Leave"));
    }
}
