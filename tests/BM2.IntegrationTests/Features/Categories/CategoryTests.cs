using System.Net;
using System.Net.Http.Json;
using BM2.Contracts.Categories;
using BM2.Contracts.Wallets;
using BM2.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace BM2.IntegrationTests.Features.Categories;

public class CategoryTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task New_wallet_has_default_categories()
    {
        var me = await CreateUserAsync("me@example.com");
        using var client = Factory.CreateClientAs(me.Id);

        var created = await (await client.PostAsJsonAsync("/api/wallets", new CreateWalletRequest("Home", "PLN"), Ct))
            .Content.ReadFromJsonAsync<WalletListItem>(Ct);
        var snapshot = await client.GetFromJsonAsync<WalletSnapshot>($"/api/wallets/{created!.Id}", Ct);

        snapshot!.Categories.Count(c => c.Type == CategoryType.Expense).ShouldBe(9);
        snapshot.Categories.Where(c => c.Type == CategoryType.Income).Select(c => c.Name).ShouldBe(["Other income", "Salary"]);
    }

    [Fact]
    public async Task Editor_creates_category()
    {
        var (client, wallet) = await MemberAsync(MemberRole.Editor);

        var response = await client.PostAsJsonAsync(
            $"/api/wallets/{wallet}/categories", new CreateCategoryRequest(" Pets ", CategoryType.Expense), Ct);
        var item = await response.Content.ReadFromJsonAsync<CategoryItem>(Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        item!.Name.ShouldBe("Pets");
        item.Type.ShouldBe(CategoryType.Expense);
    }

    [Fact]
    public async Task Same_name_with_other_type_is_allowed()
    {
        var (client, wallet) = await MemberAsync(MemberRole.Owner);
        await Data.CategoryAsync(wallet, "Gifts", CategoryType.Expense);

        var response = await client.PostAsJsonAsync(
            $"/api/wallets/{wallet}/categories", new CreateCategoryRequest("Gifts", CategoryType.Income), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Duplicate_name_and_type_returns_409()
    {
        var (client, wallet) = await MemberAsync(MemberRole.Owner);
        await Data.CategoryAsync(wallet, "Gifts", CategoryType.Expense);

        var response = await client.PostAsJsonAsync(
            $"/api/wallets/{wallet}/categories", new CreateCategoryRequest("Gifts", CategoryType.Expense), Ct);
        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        problem!.Errors.ShouldContainKey("Name");
    }

    [Fact]
    public async Task Owner_renames_and_archives_category()
    {
        var (client, wallet) = await MemberAsync(MemberRole.Owner);
        var category = await Data.CategoryAsync(wallet, "Fun");

        var response = await client.PutAsJsonAsync(
            $"/api/wallets/{wallet}/categories/{category.Id}", new UpdateCategoryRequest("Entertainment", true), Ct);
        var snapshot = await client.GetFromJsonAsync<WalletSnapshot>($"/api/wallets/{wallet}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        snapshot!.Categories.Single().ShouldBe(new CategoryItem(category.Id, "Entertainment", CategoryType.Expense, true));
    }

    [Fact]
    public async Task Unused_category_can_be_deleted()
    {
        var (client, wallet) = await MemberAsync(MemberRole.Editor);
        var category = await Data.CategoryAsync(wallet);

        var response = await client.DeleteAsync($"/api/wallets/{wallet}/categories/{category.Id}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Category_from_other_wallet_returns_404()
    {
        var (client, wallet) = await MemberAsync(MemberRole.Owner);
        var other = await CreateUserAsync("other@example.com");
        var otherWallet = await Data.WalletAsync(other.Id);
        var foreign = await Data.CategoryAsync(otherWallet.Id);

        var response = await client.DeleteAsync($"/api/wallets/{wallet}/categories/{foreign.Id}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Limited_user_sees_categories_read_only()
    {
        var owner = await CreateUserAsync("owner@example.com");
        var me = await CreateUserAsync("me@example.com");
        var wallet = await Data.WalletAsync(owner.Id);
        var account = await Data.AccountAsync(wallet.Id);
        await Data.CategoryAsync(wallet.Id);
        await Data.ShareAsync(account.Id, me.Id, MemberRole.Editor);
        using var client = Factory.CreateClientAs(me.Id);

        var snapshot = await client.GetFromJsonAsync<WalletSnapshot>($"/api/wallets/{wallet.Id}", Ct);
        var create = await client.PostAsJsonAsync(
            $"/api/wallets/{wallet.Id}/categories", new CreateCategoryRequest("Mine", CategoryType.Expense), Ct);

        snapshot!.Categories.ShouldNotBeEmpty();
        snapshot.Permissions.ManageCategories.ShouldBeFalse();
        create.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    private async Task<(HttpClient Client, Guid WalletId)> MemberAsync(MemberRole role)
    {
        var owner = await CreateUserAsync("owner@example.com");
        var wallet = await Data.WalletAsync(owner.Id);
        if (role == MemberRole.Owner)
        {
            return (Factory.CreateClientAs(owner.Id), wallet.Id);
        }

        var member = await CreateUserAsync("member@example.com");
        await Data.MemberAsync(wallet.Id, member.Id, role);
        return (Factory.CreateClientAs(member.Id), wallet.Id);
    }
}
