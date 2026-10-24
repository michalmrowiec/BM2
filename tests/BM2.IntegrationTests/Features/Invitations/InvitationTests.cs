using System.Net;
using System.Net.Http.Json;
using BM2.Contracts.Invitations;
using BM2.Contracts.Members;
using BM2.Contracts.Wallets;
using BM2.IntegrationTests.Infrastructure;

namespace BM2.IntegrationTests.Features.Invitations;

public class InvitationTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Accepted_wallet_invitation_grants_role()
    {
        var s = await SetupAsync();
        var token = await InviteAsync(s, new CreateInvitationRequest(MemberRole.Editor, null));

        var response = await s.Guest.PostAsync($"/api/invitations/{token}/accept", null, Ct);
        var accepted = await response.Content.ReadFromJsonAsync<AcceptedInvitation>(Ct);
        var snapshot = await s.Guest.GetFromJsonAsync<WalletSnapshot>($"/api/wallets/{s.Wallet}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        accepted!.WalletId.ShouldBe(s.Wallet);
        snapshot!.Role.ShouldBe(MemberRole.Editor);
    }

    [Fact]
    public async Task Accepted_account_invitation_shares_only_that_account()
    {
        var s = await SetupAsync();
        var joint = await Data.AccountAsync(s.Wallet, "Joint");
        await Data.AccountAsync(s.Wallet, "Private");
        var token = await InviteAsync(s, new CreateInvitationRequest(MemberRole.Viewer, joint.Id));

        await s.Guest.PostAsync($"/api/invitations/{token}/accept", null, Ct);
        var snapshot = await s.Guest.GetFromJsonAsync<WalletSnapshot>($"/api/wallets/{s.Wallet}", Ct);

        snapshot!.Role.ShouldBeNull();
        snapshot.Accounts.Select(a => a.Id).ShouldBe([joint.Id]);
    }

    [Fact]
    public async Task Owner_accepting_editor_invitation_stays_owner()
    {
        var s = await SetupAsync();
        var token = await InviteAsync(s, new CreateInvitationRequest(MemberRole.Editor, null));

        var response = await s.Owner.PostAsync($"/api/invitations/{token}/accept", null, Ct);
        var snapshot = await s.Owner.GetFromJsonAsync<WalletSnapshot>($"/api/wallets/{s.Wallet}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        snapshot!.Role.ShouldBe(MemberRole.Owner);
    }

    [Fact]
    public async Task Second_accept_returns_410()
    {
        var s = await SetupAsync();
        var token = await InviteAsync(s, new CreateInvitationRequest(MemberRole.Viewer, null));
        await s.Guest.PostAsync($"/api/invitations/{token}/accept", null, Ct);
        var other = await CreateUserAsync("other@example.com");
        using var otherClient = Factory.CreateClientAs(other.Id);

        var response = await otherClient.PostAsync($"/api/invitations/{token}/accept", null, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Gone);
        (await otherClient.GetAsync($"/api/wallets/{s.Wallet}", Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Expired_invitation_returns_410()
    {
        var s = await SetupAsync();
        var token = await InviteAsync(s, new CreateInvitationRequest(MemberRole.Viewer, null));
        Factory.Time.Advance(TimeSpan.FromDays(8));

        var response = await s.Guest.PostAsync($"/api/invitations/{token}/accept", null, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Gone);
    }

    [Fact]
    public async Task Revoked_invitation_returns_410()
    {
        var s = await SetupAsync();
        var created = await CreateAsync(s, new CreateInvitationRequest(MemberRole.Viewer, null));

        var revoke = await s.Owner.DeleteAsync($"/api/wallets/{s.Wallet}/invitations/{created.Id}", Ct);
        var preview = await s.Guest.GetAsync($"/api/invitations/{created.Token}", Ct);

        revoke.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        preview.StatusCode.ShouldBe(HttpStatusCode.Gone);
    }

    [Fact]
    public async Task Invitation_from_a_removed_owner_returns_410()
    {
        var s = await SetupAsync();
        var coOwner = await CreateUserAsync("coowner@example.com");
        await Data.MemberAsync(s.Wallet, coOwner.Id, MemberRole.Owner);
        var (_, token) = await Data.InvitationAsync(s.Wallet, coOwner.Id, MemberRole.Owner);
        await s.Owner.DeleteAsync($"/api/wallets/{s.Wallet}/members/{coOwner.Id}", Ct);

        var preview = await s.Guest.GetAsync($"/api/invitations/{token}", Ct);
        var accept = await s.Guest.PostAsync($"/api/invitations/{token}/accept", null, Ct);

        preview.StatusCode.ShouldBe(HttpStatusCode.Gone);
        accept.StatusCode.ShouldBe(HttpStatusCode.Gone);
        (await s.Guest.GetAsync($"/api/wallets/{s.Wallet}", Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Invitation_from_a_demoted_owner_returns_410()
    {
        var s = await SetupAsync();
        var coOwner = await CreateUserAsync("coowner@example.com");
        await Data.MemberAsync(s.Wallet, coOwner.Id, MemberRole.Owner);
        var (_, token) = await Data.InvitationAsync(s.Wallet, coOwner.Id, MemberRole.Editor);
        await s.Owner.PutAsJsonAsync($"/api/wallets/{s.Wallet}/members/{coOwner.Id}", new UpdateRoleRequest(MemberRole.Viewer), Ct);

        var accept = await s.Guest.PostAsync($"/api/invitations/{token}/accept", null, Ct);

        accept.StatusCode.ShouldBe(HttpStatusCode.Gone);
    }

    [Theory]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    [InlineData("not-a-token")]
    public async Task Unknown_or_malformed_token_returns_404(string token)
    {
        var s = await SetupAsync();

        var response = await s.Guest.GetAsync($"/api/invitations/{token}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Preview_shows_wallet_and_inviter()
    {
        var s = await SetupAsync();
        var joint = await Data.AccountAsync(s.Wallet, "Joint");
        var token = await InviteAsync(s, new CreateInvitationRequest(MemberRole.Viewer, joint.Id));

        var preview = await s.Guest.GetFromJsonAsync<InvitationPreview>($"/api/invitations/{token}", Ct);

        preview.ShouldBe(new InvitationPreview(s.Wallet, "Home", "owner@example.com", MemberRole.Viewer, "Joint"));
    }

    [Fact]
    public async Task Pending_invitations_are_listed_for_owner_only()
    {
        var s = await SetupAsync();
        await InviteAsync(s, new CreateInvitationRequest(MemberRole.Viewer, null));
        var viewer = await CreateUserAsync("viewer@example.com");
        await Data.MemberAsync(s.Wallet, viewer.Id, MemberRole.Viewer);
        using var viewerClient = Factory.CreateClientAs(viewer.Id);

        var ownerView = await s.Owner.GetFromJsonAsync<MembersResponse>($"/api/wallets/{s.Wallet}/members", Ct);
        var viewerView = await viewerClient.GetFromJsonAsync<MembersResponse>($"/api/wallets/{s.Wallet}/members", Ct);

        ownerView!.Invitations.ShouldHaveSingleItem().Role.ShouldBe(MemberRole.Viewer);
        viewerView!.Invitations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Used_invitations_are_not_listed()
    {
        var s = await SetupAsync();
        var token = await InviteAsync(s, new CreateInvitationRequest(MemberRole.Viewer, null));
        await s.Guest.PostAsync($"/api/invitations/{token}/accept", null, Ct);

        var members = await s.Owner.GetFromJsonAsync<MembersResponse>($"/api/wallets/{s.Wallet}/members", Ct);

        members!.Invitations.ShouldBeEmpty();
        members.Members.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Account_from_other_wallet_returns_404()
    {
        var s = await SetupAsync();
        var stranger = await CreateUserAsync("stranger@example.com");
        var foreignWallet = await Data.WalletAsync(stranger.Id);
        var foreign = await Data.AccountAsync(foreignWallet.Id);

        var response = await s.Owner.PostAsJsonAsync(
            $"/api/wallets/{s.Wallet}/invitations", new CreateInvitationRequest(MemberRole.Viewer, foreign.Id), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Account_invitation_cannot_be_owner()
    {
        var s = await SetupAsync();
        var joint = await Data.AccountAsync(s.Wallet, "Joint");

        var response = await s.Owner.PostAsJsonAsync(
            $"/api/wallets/{s.Wallet}/invitations", new CreateInvitationRequest(MemberRole.Owner, joint.Id), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Anonymous_preview_returns_401()
    {
        var s = await SetupAsync();
        var token = await InviteAsync(s, new CreateInvitationRequest(MemberRole.Viewer, null));
        using var anonymous = Factory.CreateAnonymousClient();

        var response = await anonymous.GetAsync($"/api/invitations/{token}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private static async Task<CreatedInvitation> CreateAsync(Setup s, CreateInvitationRequest request)
    {
        var response = await s.Owner.PostAsJsonAsync($"/api/wallets/{s.Wallet}/invitations", request, Ct);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<CreatedInvitation>(Ct))!;
    }

    private static async Task<string> InviteAsync(Setup s, CreateInvitationRequest request) => (await CreateAsync(s, request)).Token;

    private async Task<Setup> SetupAsync()
    {
        var owner = await CreateUserAsync("owner@example.com");
        var guest = await CreateUserAsync("guest@example.com");
        var wallet = await Data.WalletAsync(owner.Id);
        return new Setup(Factory.CreateClientAs(owner.Id), Factory.CreateClientAs(guest.Id), wallet.Id);
    }

    private sealed record Setup(HttpClient Owner, HttpClient Guest, Guid Wallet);
}
