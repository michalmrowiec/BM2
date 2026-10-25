using System.Net;
using System.Net.Http.Json;
using BM2.Contracts.Tags;
using BM2.Contracts.Wallets;
using BM2.IntegrationTests.Infrastructure;

namespace BM2.IntegrationTests.Features.Tags;

public class TagTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Editor_creates_and_archives_tag()
    {
        var (client, wallet) = await EditorAsync();

        var created = await (await client.PostAsJsonAsync($"/api/wallets/{wallet}/tags", new CreateTagRequest(" Volvo "), Ct))
            .Content.ReadFromJsonAsync<TagItem>(Ct);
        await client.PutAsJsonAsync($"/api/wallets/{wallet}/tags/{created!.Id}", new UpdateTagRequest("Volvo", true), Ct);
        var snapshot = await client.GetFromJsonAsync<WalletSnapshot>($"/api/wallets/{wallet}", Ct);

        snapshot!.Tags.ShouldHaveSingleItem().ShouldBe(new TagItem(created.Id, "Volvo", true));
    }

    [Fact]
    public async Task Duplicate_tag_returns_409()
    {
        var (client, wallet) = await EditorAsync();
        await Data.TagAsync(wallet, "House");

        var response = await client.PostAsJsonAsync($"/api/wallets/{wallet}/tags", new CreateTagRequest("House"), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Tag_from_other_wallet_returns_404()
    {
        var (client, wallet) = await EditorAsync();
        var other = await CreateUserAsync("other@example.com");
        var otherWallet = await Data.WalletAsync(other.Id);
        var foreign = await Data.TagAsync(otherWallet.Id);

        var response = await client.PutAsJsonAsync($"/api/wallets/{wallet}/tags/{foreign.Id}", new UpdateTagRequest("Mine", false), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Tag_name_is_at_most_30_characters()
    {
        var (client, wallet) = await EditorAsync();

        var response = await client.PostAsJsonAsync($"/api/wallets/{wallet}/tags", new CreateTagRequest(new string('x', 31)), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private async Task<(HttpClient Client, Guid WalletId)> EditorAsync()
    {
        var owner = await CreateUserAsync("owner@example.com");
        var editor = await CreateUserAsync("editor@example.com");
        var wallet = await Data.WalletAsync(owner.Id);
        await Data.MemberAsync(wallet.Id, editor.Id, MemberRole.Editor);
        return (Factory.CreateClientAs(editor.Id), wallet.Id);
    }
}
