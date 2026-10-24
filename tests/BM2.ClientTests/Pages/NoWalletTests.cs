using System.Net;
using BM2.Client.Pages;
using BM2.ClientTests.Infrastructure;
using BM2.Contracts.Wallets;
using Microsoft.AspNetCore.Components;

namespace BM2.ClientTests.Pages;

public class NoWalletTests : BunitContext
{
    private readonly StubApi _api;

    public NoWalletTests()
    {
        AddAuthorization().SetAuthorized("me@example.com");
        _api = this.UseBm2Client();
    }

    [Fact]
    public void Accounts_points_to_creating_a_wallet_when_there_is_none() => AssertPointsToWalletCreation<Accounts>();

    [Fact]
    public void Records_points_to_creating_a_wallet_when_there_is_none() => AssertPointsToWalletCreation<Records>();

    [Fact]
    public void Record_editor_points_to_creating_a_wallet_when_there_is_none() => AssertPointsToWalletCreation<RecordEditor>();

    [Fact]
    public void Failed_wallet_load_offers_a_retry_that_loads_again()
    {
        _api.On(HttpMethod.Get, "api/wallets", _ => StubApi.Problem(HttpStatusCode.InternalServerError));
        var cut = Render<Categories>();
        cut.WaitForAssertion(() => cut.Markup.ShouldContain("Try again"));
        _api.WithWallets(Snapshots.Owner(Guid.NewGuid()));

        cut.FindAll("button").Single(b => b.TextContent.Contains("Try again")).Click();

        cut.WaitForAssertion(() => cut.Markup.ShouldContain("Expenses"));
        cut.FindAll("[aria-busy=true]").ShouldBeEmpty();
    }

    private void AssertPointsToWalletCreation<TPage>() where TPage : IComponent
    {
        _api.OnGet("api/wallets", new List<WalletListItem>());

        var cut = Render<TPage>();

        cut.WaitForAssertion(() => cut.Markup.ShouldContain("Create a wallet"));
        cut.FindAll("[aria-busy=true]").ShouldBeEmpty();
    }
}
