using System.Net;
using BM2.Contracts.Wallets;
using BM2.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BM2.IntegrationTests.Features.Invitations;

public class InviteRegistrationTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Register_page_with_unknown_invite_is_404_outside_development()
    {
        using var client = Factory.CreateAnonymousClient();

        var response = await client.GetAsync("/Account/Register?invite=AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Register_page_with_expired_invite_is_404()
    {
        var token = await InvitationTokenAsync();
        Factory.Time.Advance(TimeSpan.FromDays(8));
        using var client = Factory.CreateAnonymousClient();

        var response = await client.GetAsync($"/Account/Register?invite={token}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Register_confirmation_page_stays_closed_with_invite()
    {
        var token = await InvitationTokenAsync();
        using var client = Factory.CreateAnonymousClient();

        var response = await client.GetAsync($"/Account/RegisterConfirmation?invite={token}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Registering_with_invite_joins_the_wallet_and_uses_up_the_link()
    {
        var (walletId, token) = await InvitationAsync();
        using var client = Factory.CreateAnonymousClient();

        using var response = await RegisterAsync(client, token, "new@example.com");

        ((int)response.StatusCode).ShouldBeInRange(300, 399);
        response.Headers.GetValues("Set-Cookie").ShouldContain(c => c.StartsWith(".AspNetCore.Identity.Application", StringComparison.Ordinal));
        await using var db = Fixture.CreateDbContext();
        var user = await db.Users.SingleAsync(u => u.Email == "new@example.com", Ct);
        user.EmailConfirmed.ShouldBeTrue();
        (await db.WalletMembers.SingleAsync(m => m.UserId == user.Id, Ct)).ShouldSatisfyAllConditions(
            m => m.WalletId.ShouldBe(walletId),
            m => m.Role.ShouldBe(MemberRole.Editor));
        (await db.Invitations.SingleAsync(Ct)).AcceptedByUserId.ShouldBe(user.Id);
    }

    [Fact]
    public async Task Invite_link_cannot_register_a_second_account()
    {
        var (_, token) = await InvitationAsync();
        using var first = Factory.CreateAnonymousClient();
        (await RegisterAsync(first, token, "first@example.com")).Dispose();
        using var second = Factory.CreateAnonymousClient();

        var response = await second.GetAsync($"/Account/Register?invite={token}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private static Task<HttpResponseMessage> RegisterAsync(HttpClient client, string token, string email) =>
        FormPost.SubmitAsync(client, $"/Account/Register?invite={token}", "register", new Dictionary<string, string>
        {
            ["Input.Email"] = email,
            ["Input.Password"] = DefaultPassword,
            ["Input.ConfirmPassword"] = DefaultPassword,
        }, Ct);

    private async Task<(Guid WalletId, string Token)> InvitationAsync()
    {
        var owner = await CreateUserAsync("owner@example.com");
        var wallet = await Data.WalletAsync(owner.Id);
        var (_, token) = await Data.InvitationAsync(wallet.Id, owner.Id, MemberRole.Editor);
        return (wallet.Id, token);
    }

    private async Task<string> InvitationTokenAsync() => (await InvitationAsync()).Token;
}
