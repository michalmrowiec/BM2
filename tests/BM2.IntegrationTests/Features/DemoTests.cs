using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using BM2.Contracts.Common;
using BM2.Contracts.Wallets;
using BM2.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BM2.IntegrationTests.Features;

public partial class DemoTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Demo_login_is_not_found_when_disabled()
    {
        using var client = Factory.CreateAnonymousClient();

        using var response = await client.PostAsync("/Account/Demo", new FormUrlEncodedContent([]), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await client.GetStringAsync("/Account/Login", Ct)).ShouldNotContain("Account/Demo");
    }

    [Fact]
    public async Task Demo_login_needs_the_antiforgery_token()
    {
        await using var demo = DemoFactory();
        using var client = Client(demo);
        (await client.GetStringAsync("/Account/Login", Ct)).ShouldContain("Account/Demo");

        using var response = await client.PostAsync("/Account/Demo", new FormUrlEncodedContent([]), Ct);

        // The 400 is re-executed as the page's error status, so check that no session started.
        response.StatusCode.ShouldNotBe(HttpStatusCode.Redirect);
        (await client.GetAsync("/api/wallets", Ct)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Demo_sees_only_its_sample_wallet_and_can_read_it()
    {
        var other = await CreateUserAsync("other@example.com");
        var otherWallet = await Data.WalletAsync(other.Id, "Private");
        await using var demo = DemoFactory();
        using var client = Client(demo);
        await LogInAsDemoAsync(client);
        await LogInAsDemoAsync(client);

        var wallets = await client.GetFromJsonAsync<List<WalletListItem>>("/api/wallets", Ct);

        var wallet = wallets.ShouldHaveSingleItem();
        wallet.Name.ShouldBe("Demo household");
        (await client.GetAsync($"/api/wallets/{wallet.Id}", Ct)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await client.GetAsync($"/api/wallets/{wallet.Id}/stats/forecast", Ct)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await client.GetAsync($"/api/wallets/{wallet.Id}/records/export?format=csv", Ct)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await client.GetAsync($"/api/wallets/{otherWallet.Id}", Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Demo_cannot_write()
    {
        await using var demo = DemoFactory();
        using var client = Client(demo);
        await LogInAsDemoAsync(client);
        var wallet = (await client.GetFromJsonAsync<List<WalletListItem>>("/api/wallets", Ct))!.Single();

        using var create = await client.PostAsJsonAsync("/api/wallets", new CreateWalletRequest("Mine", "PLN"), Ct);
        using var rename = await client.PutAsJsonAsync($"/api/wallets/{wallet.Id}", new UpdateWalletRequest("Renamed", "PLN"), Ct);
        using var delete = await client.DeleteAsync($"/api/wallets/{wallet.Id}", Ct);
        using var password = await client.PostAsync("/Account/Manage/ChangePassword", new FormUrlEncodedContent([]), Ct);

        create.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await create.Content.ReadFromJsonAsync<ProblemDetails>(Ct))!.Detail.ShouldBe(Demo.ReadOnlyMessage);
        rename.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        delete.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        password.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await client.GetFromJsonAsync<List<WalletListItem>>("/api/wallets", Ct))!.ShouldHaveSingleItem().Name.ShouldBe("Demo household");
    }

    private WebApplicationFactory<Program> DemoFactory() =>
        Factory.WithWebHostBuilder(builder => builder.UseSetting("Demo:Enabled", "true"));

    private static HttpClient Client(WebApplicationFactory<Program> factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });

    private static async Task LogInAsDemoAsync(HttpClient client)
    {
        var page = await client.GetStringAsync("/Account/Login", Ct);
        var token = AntiforgeryToken().Match(page).Groups[1].Value;
        using var form = new FormUrlEncodedContent(new Dictionary<string, string> { ["__RequestVerificationToken"] = token });

        using var response = await client.PostAsync("/Account/Demo", form, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
    }

    [GeneratedRegex("action=\"Account/Demo\"[^>]*>\\s*<input[^>]*value=\"([^\"]+)\"")]
    private static partial Regex AntiforgeryToken();
}
