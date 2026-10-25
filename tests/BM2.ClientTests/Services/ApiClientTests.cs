using System.Net;
using BM2.Client.Services;
using BM2.ClientTests.Infrastructure;
using BM2.Contracts.Wallets;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BM2.ClientTests.Services;

public class ApiClientTests : BunitContext
{
    private readonly StubApi _api;

    public ApiClientTests() => _api = this.UseBm2Client();

    private ApiClient Client => Services.GetRequiredService<ApiClient>();

    [Fact]
    public async Task Returns_value_on_success()
    {
        _api.OnGet("api/wallets", new List<WalletListItem> { new(Guid.NewGuid(), "Home", "PLN", MemberRole.Owner) });

        var result = await Client.GetAsync<List<WalletListItem>>("api/wallets");

        result.Ok.ShouldBeTrue();
        result.Value!.Single().Name.ShouldBe("Home");
    }

    [Fact]
    public async Task Maps_validation_problem_to_field_errors()
    {
        _api.On(HttpMethod.Post, "api/wallets", _ => StubApi.Problem(
            HttpStatusCode.BadRequest, new Dictionary<string, string[]> { ["Name"] = ["Required"] }));

        var result = await Client.PostAsync<WalletListItem>("api/wallets", new CreateWalletRequest("", "PLN"));

        result.Ok.ShouldBeFalse();
        result.Problem!.Status.ShouldBe(400);
        result.Problem.Errors["Name"].ShouldBe(["Required"]);
    }

    [Fact]
    public async Task Problem_without_errors_has_empty_dictionary()
    {
        _api.On(HttpMethod.Delete, "api/wallets/1", _ => StubApi.Problem(HttpStatusCode.Forbidden));

        var result = await Client.DeleteAsync("api/wallets/1");

        result.Problem!.Status.ShouldBe(403);
        result.Problem.Errors.ShouldBeEmpty();
    }

    [Fact]
    public async Task Redirects_to_login_on_401()
    {
        _api.On(HttpMethod.Get, "api/wallets", _ => new HttpResponseMessage(HttpStatusCode.Unauthorized));

        var result = await Client.GetAsync<List<WalletListItem>>("api/wallets");

        result.Problem!.Status.ShouldBe(401);
        var navigation = (BunitNavigationManager)Services.GetRequiredService<NavigationManager>();
        navigation.History.ShouldContain(entry => entry.Uri.Contains("Account/Login") && entry.Options.ForceLoad);
    }

    [Fact]
    public async Task Network_failure_is_status_zero()
    {
        _api.On(HttpMethod.Get, "api/wallets", _ => throw new HttpRequestException("offline"));

        var result = await Client.GetAsync<List<WalletListItem>>("api/wallets");

        result.Problem!.Status.ShouldBe(0);
    }
}
