using System.Net;
using System.Net.Http.Json;
using BM2.Contracts.Accounts;
using BM2.Contracts.Wallets;
using BM2.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BM2.IntegrationTests.Features.Accounts;

public class AccountTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Owner_creates_account_visible_in_snapshot()
    {
        var (client, wallet) = await OwnerWithWalletAsync();

        var response = await client.PostAsJsonAsync($"/api/wallets/{wallet}/accounts", new CreateAccountRequest(" Bank ", "EUR", 100m), Ct);
        var created = await response.Content.ReadFromJsonAsync<AccountItem>(Ct);
        var snapshot = await client.GetFromJsonAsync<WalletSnapshot>($"/api/wallets/{wallet}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        snapshot!.Accounts.ShouldHaveSingleItem().ShouldBe(new AccountItem(created!.Id, "Bank", "EUR", 100m, 100m, false, true));
    }

    [Fact]
    public async Task Duplicate_name_in_wallet_returns_409()
    {
        var (client, wallet) = await OwnerWithWalletAsync();
        await Data.AccountAsync(wallet, "Cash");

        var response = await client.PostAsJsonAsync($"/api/wallets/{wallet}/accounts", new CreateAccountRequest("Cash", "PLN", 0), Ct);
        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        problem!.Errors.ShouldContainKey("Name");
    }

    [Fact]
    public async Task Same_name_in_other_wallet_is_allowed()
    {
        var (client, wallet) = await OwnerWithWalletAsync();
        var other = await CreateUserAsync("other@example.com");
        var otherWallet = await Data.WalletAsync(other.Id);
        await Data.AccountAsync(otherWallet.Id, "Cash");

        var response = await client.PostAsJsonAsync($"/api/wallets/{wallet}/accounts", new CreateAccountRequest("Cash", "PLN", 0), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Renaming_to_own_name_is_not_a_conflict()
    {
        var (client, wallet) = await OwnerWithWalletAsync();
        var account = await Data.AccountAsync(wallet, "Cash");

        var response = await client.PutAsJsonAsync(
            $"/api/wallets/{wallet}/accounts/{account.Id}", new UpdateAccountRequest("Cash", "EUR", 5m, false), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Account_from_other_wallet_returns_404()
    {
        var (client, wallet) = await OwnerWithWalletAsync();
        var other = await CreateUserAsync("other@example.com");
        var otherWallet = await Data.WalletAsync(other.Id);
        var foreign = await Data.AccountAsync(otherWallet.Id, "Theirs");

        var response = await client.PutAsJsonAsync(
            $"/api/wallets/{wallet}/accounts/{foreign.Id}", new UpdateAccountRequest("Mine", "PLN", 0, false), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        await using var db = Fixture.CreateDbContext();
        (await db.Accounts.SingleAsync(a => a.Id == foreign.Id, Ct)).Name.ShouldBe("Theirs");
    }

    [Fact]
    public async Task Archived_account_stays_in_snapshot_marked_archived()
    {
        var (client, wallet) = await OwnerWithWalletAsync();
        var account = await Data.AccountAsync(wallet, "Old bank");

        await client.PutAsJsonAsync($"/api/wallets/{wallet}/accounts/{account.Id}", new UpdateAccountRequest("Old bank", "PLN", 0, true), Ct);
        var snapshot = await client.GetFromJsonAsync<WalletSnapshot>($"/api/wallets/{wallet}", Ct);

        snapshot!.Accounts.ShouldHaveSingleItem().IsArchived.ShouldBeTrue();
    }

    [Fact]
    public async Task Deleting_account_removes_its_shares()
    {
        var (client, wallet) = await OwnerWithWalletAsync();
        var account = await Data.AccountAsync(wallet);
        var friend = await CreateUserAsync("friend@example.com");
        await Data.ShareAsync(account.Id, friend.Id, MemberRole.Viewer);

        var response = await client.DeleteAsync($"/api/wallets/{wallet}/accounts/{account.Id}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        await using var db = Fixture.CreateDbContext();
        (await db.Accounts.CountAsync(Ct)).ShouldBe(0);
        (await db.AccountMembers.CountAsync(Ct)).ShouldBe(0);
    }

    [Fact]
    public async Task Invalid_account_returns_validation_problem()
    {
        var (client, wallet) = await OwnerWithWalletAsync();

        var response = await client.PostAsJsonAsync($"/api/wallets/{wallet}/accounts", new CreateAccountRequest("", "XYZ", 1.005m), Ct);
        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem!.Errors.Keys.ShouldBe(["Name", "Currency", "OpeningBalance"], ignoreOrder: true);
    }

    private async Task<(HttpClient Client, Guid WalletId)> OwnerWithWalletAsync()
    {
        var owner = await CreateUserAsync("owner@example.com");
        var wallet = await Data.WalletAsync(owner.Id);
        return (Factory.CreateClientAs(owner.Id), wallet.Id);
    }
}
