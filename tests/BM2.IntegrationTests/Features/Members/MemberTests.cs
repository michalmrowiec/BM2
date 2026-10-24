using System.Net;
using System.Net.Http.Json;
using BM2.Contracts.Members;
using BM2.Contracts.Wallets;
using BM2.IntegrationTests.Infrastructure;

namespace BM2.IntegrationTests.Features.Members;

public class MemberTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Viewer_sees_members_without_invitations()
    {
        var s = await SetupAsync(MemberRole.Viewer);

        var members = await s.MemberClient.GetFromJsonAsync<MembersResponse>($"{s.Base}/members", Ct);

        members!.Members.Select(m => (m.Email, m.Role, m.IsMe)).ShouldBe(
            [("owner@example.com", MemberRole.Owner, false), ("member@example.com", MemberRole.Viewer, true)]);
        members.Invitations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Members_list_includes_account_shares()
    {
        var s = await SetupAsync(MemberRole.Viewer);
        var account = await Data.AccountAsync(s.Wallet, "Joint");
        var friend = await CreateUserAsync("friend@example.com");
        await Data.ShareAsync(account.Id, friend.Id, MemberRole.Editor);

        var members = await s.OwnerClient.GetFromJsonAsync<MembersResponse>($"{s.Base}/members", Ct);

        members!.AccountShares.ShouldHaveSingleItem()
            .ShouldBe(new AccountShareItem(account.Id, "Joint", friend.Id, "friend@example.com", MemberRole.Editor, false));
    }

    [Fact]
    public async Task Owner_promotes_editor_to_owner()
    {
        var s = await SetupAsync(MemberRole.Editor);

        var response = await s.OwnerClient.PutAsJsonAsync($"{s.Base}/members/{s.MemberId}", new UpdateRoleRequest(MemberRole.Owner), Ct);
        var snapshot = await s.MemberClient.GetFromJsonAsync<WalletSnapshot>(s.Base, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        snapshot!.Role.ShouldBe(MemberRole.Owner);
    }

    [Fact]
    public async Task Last_owner_cannot_demote_self()
    {
        var s = await SetupAsync(MemberRole.Editor);

        var response = await s.OwnerClient.PutAsJsonAsync($"{s.Base}/members/{s.OwnerId}", new UpdateRoleRequest(MemberRole.Editor), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Last_owner_cannot_leave()
    {
        var s = await SetupAsync(MemberRole.Editor);

        var response = await s.OwnerClient.DeleteAsync($"{s.Base}/members/{s.OwnerId}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Member_can_leave_wallet()
    {
        var s = await SetupAsync(MemberRole.Editor);

        var response = await s.MemberClient.DeleteAsync($"{s.Base}/members/{s.MemberId}", Ct);
        var after = await s.MemberClient.GetAsync(s.Base, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        after.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Owner_removes_member()
    {
        var s = await SetupAsync(MemberRole.Viewer);

        var response = await s.OwnerClient.DeleteAsync($"{s.Base}/members/{s.MemberId}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await s.MemberClient.GetAsync(s.Base, Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Editor_cannot_remove_other_member()
    {
        var s = await SetupAsync(MemberRole.Editor);

        var response = await s.MemberClient.DeleteAsync($"{s.Base}/members/{s.OwnerId}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Removing_unknown_member_returns_404()
    {
        var s = await SetupAsync(MemberRole.Editor);
        var stranger = await CreateUserAsync("stranger@example.com");

        var response = await s.OwnerClient.DeleteAsync($"{s.Base}/members/{stranger.Id}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Shared_account_user_can_leave_share()
    {
        var (s, account, friend) = await SharedAsync();

        var response = await friend.DeleteAsync($"{s.Base}/accounts/{account}/members/{s.FriendId}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await friend.GetAsync(s.Base, Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Shared_account_user_cannot_list_members()
    {
        var (s, _, friend) = await SharedAsync();

        var response = await friend.GetAsync($"{s.Base}/members", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Owner_changes_account_share_role()
    {
        var (s, account, friend) = await SharedAsync();

        var response = await s.OwnerClient.PutAsJsonAsync(
            $"{s.Base}/accounts/{account}/members/{s.FriendId}", new UpdateRoleRequest(MemberRole.Editor), Ct);
        var snapshot = await friend.GetFromJsonAsync<WalletSnapshot>(s.Base, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        snapshot!.Accounts.Single().CanWrite.ShouldBeTrue();
    }

    [Fact]
    public async Task Account_share_cannot_be_owner()
    {
        var (s, account, _) = await SharedAsync();

        var response = await s.OwnerClient.PutAsJsonAsync(
            $"{s.Base}/accounts/{account}/members/{s.FriendId}", new UpdateRoleRequest(MemberRole.Owner), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Account_from_other_wallet_returns_404()
    {
        var (s, _, _) = await SharedAsync();
        var stranger = await CreateUserAsync("stranger@example.com");
        var foreignWallet = await Data.WalletAsync(stranger.Id);
        var foreign = await Data.AccountAsync(foreignWallet.Id);

        var response = await s.OwnerClient.DeleteAsync($"{s.Base}/accounts/{foreign.Id}/members/{s.FriendId}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private async Task<Setup> SetupAsync(MemberRole memberRole)
    {
        var owner = await CreateUserAsync("owner@example.com");
        var member = await CreateUserAsync("member@example.com");
        var wallet = await Data.WalletAsync(owner.Id);
        await Data.MemberAsync(wallet.Id, member.Id, memberRole);
        return new Setup(
            Factory.CreateClientAs(owner.Id), Factory.CreateClientAs(member.Id), owner.Id, member.Id, Guid.Empty, wallet.Id);
    }

    private async Task<(Setup Setup, Guid Account, HttpClient Friend)> SharedAsync()
    {
        var s = await SetupAsync(MemberRole.Viewer);
        var account = await Data.AccountAsync(s.Wallet, "Joint");
        var friend = await CreateUserAsync("friend@example.com");
        await Data.ShareAsync(account.Id, friend.Id, MemberRole.Viewer);
        return (s with { FriendId = friend.Id }, account.Id, Factory.CreateClientAs(friend.Id));
    }

    private sealed record Setup(HttpClient OwnerClient, HttpClient MemberClient, Guid OwnerId, Guid MemberId, Guid FriendId, Guid Wallet)
    {
        public string Base => $"/api/wallets/{Wallet}";
    }
}
