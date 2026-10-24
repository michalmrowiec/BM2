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
    public async Task Registering_with_invite_creates_confirmed_user_and_returns_to_invite()
    {
        var token = await InvitationTokenAsync();
        using var client = Factory.CreateAnonymousClient();

        using var response = await FormPost.SubmitAsync(client, $"/Account/Register?invite={token}", "register", new Dictionary<string, string>
        {
            ["Input.Email"] = "new@example.com",
            ["Input.Password"] = DefaultPassword,
            ["Input.ConfirmPassword"] = DefaultPassword,
        }, Ct);

        ((int)response.StatusCode).ShouldBeInRange(300, 399);
        response.Headers.Location!.OriginalString.ShouldEndWith($"invite/{token}");
        response.Headers.GetValues("Set-Cookie").ShouldContain(c => c.StartsWith(".AspNetCore.Identity.Application", StringComparison.Ordinal));
        await using var db = Fixture.CreateDbContext();
        (await db.Users.SingleAsync(u => u.Email == "new@example.com", Ct)).EmailConfirmed.ShouldBeTrue();
    }

    private async Task<string> InvitationTokenAsync()
    {
        var owner = await CreateUserAsync("owner@example.com");
        var wallet = await Data.WalletAsync(owner.Id);
        var (_, token) = await Data.InvitationAsync(wallet.Id, owner.Id, MemberRole.Editor);
        return token;
    }
}
