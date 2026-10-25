using System.Net;
using BM2.Client.Pages;
using BM2.ClientTests.Infrastructure;
using BM2.Contracts.Invitations;
using BM2.Contracts.Wallets;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BM2.ClientTests.Pages;

public class InviteTests : BunitContext
{
    private const string Token = "TOKEN123";
    private static readonly Guid WalletId = Guid.NewGuid();

    private readonly BunitAuthorizationContext _auth;
    private readonly StubApi _api;

    public InviteTests()
    {
        _auth = AddAuthorization();
        _api = this.UseBm2Client();
    }

    private IRenderedComponent<Invite> RenderInvite() => Render<Invite>(p => p.Add(x => x.Token, Token));

    [Fact]
    public void Anonymous_user_gets_login_and_register_links_with_token()
    {
        var cut = RenderInvite();

        cut.Find("a[href^='Account/Register']").GetAttribute("href").ShouldBe($"Account/Register?invite={Token}");
        cut.Find("a[href^='Account/Login']").GetAttribute("href").ShouldBe($"Account/Login?ReturnUrl=%2Finvite%2F{Token}");
        _api.Requests.ShouldBeEmpty();
    }

    [Fact]
    public void Signed_in_user_sees_preview()
    {
        _auth.SetAuthorized("me@example.com");
        _api.OnGet($"api/invitations/{Token}", new InvitationPreview(WalletId, "Family", "owner@example.com", MemberRole.Editor, null));

        var cut = RenderInvite();

        cut.WaitForAssertion(() => cut.Markup.ShouldContain("Family"));
        cut.Markup.ShouldContain("owner@example.com");
        cut.Markup.ShouldContain("Editor");
    }

    [Fact]
    public void Joining_selects_wallet_and_goes_home()
    {
        _auth.SetAuthorized("me@example.com");
        _api.OnGet($"api/invitations/{Token}", new InvitationPreview(WalletId, "Family", "owner@example.com", MemberRole.Editor, null))
            .On(HttpMethod.Post, $"api/invitations/{Token}/accept", _ => StubApi.Json(new AcceptedInvitation(WalletId)))
            .WithWallets(Snapshots.Owner(WalletId, "Family"));
        var cut = RenderInvite();

        cut.WaitForElement("button.btn-primary").Click();

        var navigation = (BunitNavigationManager)Services.GetRequiredService<NavigationManager>();
        cut.WaitForAssertion(() => navigation.Uri.ShouldBe("http://localhost/"));
        JSInterop.VerifyInvoke("localStorage.setItem").Arguments.ShouldBe(["bm2-wallet", WalletId.ToString()]);
    }

    [Fact]
    public void Used_invitation_shows_gone_message()
    {
        _auth.SetAuthorized("me@example.com");
        _api.On(HttpMethod.Get, $"api/invitations/{Token}", _ => StubApi.Problem(HttpStatusCode.Gone));

        var cut = RenderInvite();

        cut.WaitForAssertion(() => cut.Markup.ShouldContain("expired or was already used"));
    }

    [Fact]
    public void Unknown_invitation_shows_missing_message()
    {
        _auth.SetAuthorized("me@example.com");

        var cut = RenderInvite();

        cut.WaitForAssertion(() => cut.Markup.ShouldContain("doesn't exist"));
    }
}
