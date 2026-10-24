using System.Net;
using System.Net.Http.Json;
using BM2.Contracts.Wallets;
using BM2.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BM2.IntegrationTests.Features.Wallets;

public class WalletTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Created_wallet_is_listed_with_owner_role()
    {
        var me = await CreateUserAsync("me@example.com");
        using var client = Factory.CreateClientAs(me.Id);

        var response = await client.PostAsJsonAsync("/api/wallets", new CreateWalletRequest(" Home ", "PLN"), Ct);
        var list = await client.GetFromJsonAsync<List<WalletListItem>>("/api/wallets", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var item = list.ShouldHaveSingleItem();
        item.Name.ShouldBe("Home");
        item.Role.ShouldBe(MemberRole.Owner);
    }

    [Fact]
    public async Task Wallet_list_contains_only_my_wallets()
    {
        var me = await CreateUserAsync("me@example.com");
        var other = await CreateUserAsync("other@example.com");
        await Data.WalletAsync(other.Id);
        using var client = Factory.CreateClientAs(me.Id);

        var list = await client.GetFromJsonAsync<List<WalletListItem>>("/api/wallets", Ct);

        list.ShouldBeEmpty();
    }

    [Fact]
    public async Task Shared_account_wallet_is_listed_without_role()
    {
        var me = await CreateUserAsync("me@example.com");
        var owner = await CreateUserAsync("owner@example.com");
        var wallet = await Data.WalletAsync(owner.Id);
        var account = await Data.AccountAsync(wallet.Id);
        await Data.ShareAsync(account.Id, me.Id, MemberRole.Viewer);
        using var client = Factory.CreateClientAs(me.Id);

        var list = await client.GetFromJsonAsync<List<WalletListItem>>("/api/wallets", Ct);

        var item = list.ShouldHaveSingleItem();
        item.Id.ShouldBe(wallet.Id);
        item.Role.ShouldBeNull();
    }

    [Fact]
    public async Task Snapshot_of_limited_user_contains_only_shared_account()
    {
        var me = await CreateUserAsync("me@example.com");
        var owner = await CreateUserAsync("owner@example.com");
        var wallet = await Data.WalletAsync(owner.Id);
        var shared = await Data.AccountAsync(wallet.Id, "Joint", openingBalance: 50m);
        await Data.AccountAsync(wallet.Id, "Private");
        await Data.ShareAsync(shared.Id, me.Id, MemberRole.Viewer);
        using var client = Factory.CreateClientAs(me.Id);

        var snapshot = await client.GetFromJsonAsync<WalletSnapshot>($"/api/wallets/{wallet.Id}", Ct);

        snapshot!.Role.ShouldBeNull();
        snapshot.Permissions.ShouldBe(new WalletPermissions(false, false, false, false, false));
        var account = snapshot.Accounts.ShouldHaveSingleItem();
        account.Id.ShouldBe(shared.Id);
        account.Balance.ShouldBe(50m);
        account.CanWrite.ShouldBeFalse();
    }

    [Fact]
    public async Task Owner_snapshot_has_full_permissions()
    {
        var me = await CreateUserAsync("me@example.com");
        var wallet = await Data.WalletAsync(me.Id);
        using var client = Factory.CreateClientAs(me.Id);

        var snapshot = await client.GetFromJsonAsync<WalletSnapshot>($"/api/wallets/{wallet.Id}", Ct);

        snapshot!.Name.ShouldBe("Home");
        snapshot.Role.ShouldBe(MemberRole.Owner);
        snapshot.Permissions.ShouldBe(new WalletPermissions(true, true, true, true, true));
    }

    [Fact]
    public async Task Invalid_wallet_returns_validation_problem()
    {
        var me = await CreateUserAsync("me@example.com");
        using var client = Factory.CreateClientAs(me.Id);

        var response = await client.PostAsJsonAsync("/api/wallets", new CreateWalletRequest("", "XYZ"), Ct);
        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem!.Errors.Keys.ShouldBe(["Name", "BaseCurrency"], ignoreOrder: true);
    }

    [Fact]
    public async Task Owner_updates_wallet()
    {
        var me = await CreateUserAsync("me@example.com");
        var wallet = await Data.WalletAsync(me.Id);
        using var client = Factory.CreateClientAs(me.Id);

        var response = await client.PutAsJsonAsync($"/api/wallets/{wallet.Id}", new UpdateWalletRequest("Family", "EUR"), Ct);
        var snapshot = await client.GetFromJsonAsync<WalletSnapshot>($"/api/wallets/{wallet.Id}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        snapshot!.Name.ShouldBe("Family");
        snapshot.BaseCurrency.ShouldBe("EUR");
    }

    [Fact]
    public async Task Deleting_wallet_removes_accounts_and_members()
    {
        var me = await CreateUserAsync("me@example.com");
        var other = await CreateUserAsync("other@example.com");
        var wallet = await Data.WalletAsync(me.Id);
        var account = await Data.AccountAsync(wallet.Id);
        await Data.ShareAsync(account.Id, other.Id, MemberRole.Viewer);
        using var client = Factory.CreateClientAs(me.Id);

        var response = await client.DeleteAsync($"/api/wallets/{wallet.Id}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        await using var db = Fixture.CreateDbContext();
        (await db.Wallets.CountAsync(Ct)).ShouldBe(0);
        (await db.Accounts.CountAsync(Ct)).ShouldBe(0);
        (await db.WalletMembers.CountAsync(Ct)).ShouldBe(0);
        (await db.AccountMembers.CountAsync(Ct)).ShouldBe(0);
    }
}
