using System.Net;
using System.Net.Http.Json;
using BM2.Contracts.Invitations;
using BM2.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BM2.IntegrationTests.Features.Invitations;

public class AppInvitationTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Others_cannot_see_or_revoke_my_invitations()
    {
        var (mine, created) = await CreateAsync();
        var other = Factory.CreateClientAs((await CreateUserAsync("other@example.com")).Id);

        var list = await other.GetFromJsonAsync<List<AppInvitationItem>>("/api/app-invitations", Ct);
        var revoke = await other.DeleteAsync($"/api/app-invitations/{created.Id}", Ct);
        var myList = await mine.GetFromJsonAsync<List<AppInvitationItem>>("/api/app-invitations", Ct);

        list.ShouldBeEmpty();
        revoke.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        myList!.Single().Status.ShouldBe(AppInvitationStatus.Active);
    }

    [Fact]
    public async Task At_most_ten_active_invitations()
    {
        var (client, _) = await CreateAsync();
        for (var i = 1; i < 10; i++)
        {
            (await client.PostAsJsonAsync("/api/app-invitations", new CreateAppInvitationRequest(null), Ct)).EnsureSuccessStatusCode();
        }

        var eleventh = await client.PostAsJsonAsync("/api/app-invitations", new CreateAppInvitationRequest(null), Ct);

        eleventh.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Registering_uses_up_the_link()
    {
        var (_, created) = await CreateAsync();
        using var first = Factory.CreateAnonymousClient();
        using var response = await RegisterAsync(first, created.Token, "new@example.com");
        using var second = Factory.CreateAnonymousClient();

        var reuse = await second.GetAsync($"/Account/Register?invite={created.Token}", Ct);

        ((int)response.StatusCode).ShouldBeInRange(300, 399);
        await using var db = Fixture.CreateDbContext();
        var user = await db.Users.SingleAsync(u => u.Email == "new@example.com", Ct);
        (await db.AppInvitations.SingleAsync(Ct)).UsedByUserId.ShouldBe(user.Id);
        (await db.WalletMembers.AnyAsync(m => m.UserId == user.Id, Ct)).ShouldBeFalse();
        reuse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Expired_link_does_not_open_registration()
    {
        var (_, created) = await CreateAsync();
        Factory.Time.Advance(TimeSpan.FromDays(8));
        using var client = Factory.CreateAnonymousClient();

        var response = await client.GetAsync($"/Account/Register?invite={created.Token}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Revoked_link_does_not_open_registration()
    {
        var (owner, created) = await CreateAsync();
        (await owner.DeleteAsync($"/api/app-invitations/{created.Id}", Ct)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        using var client = Factory.CreateAnonymousClient();

        var response = await client.GetAsync($"/Account/Register?invite={created.Token}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Link_for_an_email_rejects_other_addresses()
    {
        var (_, created) = await CreateAsync("friend@example.com");
        using var client = Factory.CreateAnonymousClient();

        (await RegisterAsync(client, created.Token, "stranger@example.com")).Dispose();

        await using var db = Fixture.CreateDbContext();
        (await db.Users.AnyAsync(u => u.Email == "stranger@example.com", Ct)).ShouldBeFalse();
        (await db.AppInvitations.SingleAsync(Ct)).UsedAt.ShouldBeNull();
    }

    private async Task<(HttpClient Client, CreatedInvitation Created)> CreateAsync(string? email = null)
    {
        var client = Factory.CreateClientAs((await CreateUserAsync("owner@example.com")).Id);
        var response = await client.PostAsJsonAsync("/api/app-invitations", new CreateAppInvitationRequest(email), Ct);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (client, (await response.Content.ReadFromJsonAsync<CreatedInvitation>(Ct))!);
    }

    private static Task<HttpResponseMessage> RegisterAsync(HttpClient client, string token, string email) =>
        FormPost.SubmitAsync(client, $"/Account/Register?invite={token}", "register", new Dictionary<string, string>
        {
            ["Input.Email"] = email,
            ["Input.Password"] = DefaultPassword,
            ["Input.ConfirmPassword"] = DefaultPassword,
        }, Ct);
}
