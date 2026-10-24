using System.Net;
using System.Net.Http.Json;
using BM2.Contracts.Wallets;
using BM2.IntegrationTests.Infrastructure;

namespace BM2.IntegrationTests.Features;

/// <summary>Every wallet endpoint: outsiders get 404, members below the required role get 403.</summary>
public class WalletEndpointAccessTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    private static readonly EndpointCase[] All =
    [
        new("GET", "/api/wallets/{w}", null, MemberRole.Viewer),
        new("PUT", "/api/wallets/{w}", _ => new UpdateWalletRequest("Renamed", "PLN"), MemberRole.Owner),
        new("DELETE", "/api/wallets/{w}", null, MemberRole.Owner),
    ];

    public static TheoryData<string> Cases => new(All.Select(c => c.Name));

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task Outsider_gets_404(string name)
    {
        var ids = await SeedAsync();
        var outsider = await CreateUserAsync("outsider@example.com");
        await Data.WalletAsync(outsider.Id, "Own");

        var response = await SendAsync(Find(name), ids, outsider.Id);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task Role_below_minimum_gets_403(string name)
    {
        var endpoint = Find(name);
        Assert.SkipWhen(endpoint.Minimum == MemberRole.Viewer, "Every member may call it.");
        var ids = await SeedAsync();
        var member = await CreateUserAsync("member@example.com");
        await Data.MemberAsync(ids.WalletId, member.Id, endpoint.Minimum - 1);

        var response = await SendAsync(endpoint, ids, member.Id);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    private static EndpointCase Find(string name) => All.Single(c => c.Name == name);

    private async Task<CaseIds> SeedAsync()
    {
        var owner = await CreateUserAsync("owner@example.com");
        var other = await CreateUserAsync("editor@example.com");
        var wallet = await Data.WalletAsync(owner.Id);
        await Data.MemberAsync(wallet.Id, other.Id, MemberRole.Editor);
        var account = await Data.AccountAsync(wallet.Id);
        return new CaseIds(wallet.Id, account.Id, other.Id);
    }

    private async Task<HttpResponseMessage> SendAsync(EndpointCase endpoint, CaseIds ids, Guid userId)
    {
        using var client = Factory.CreateClientAs(userId);
        using var request = new HttpRequestMessage(new HttpMethod(endpoint.Method), ids.Fill(endpoint.Path));
        if (endpoint.Body?.Invoke(ids) is { } body)
        {
            request.Content = JsonContent.Create(body, body.GetType());
        }

        return await client.SendAsync(request, Ct);
    }

    public sealed record EndpointCase(string Method, string Path, Func<CaseIds, object>? Body, MemberRole Minimum)
    {
        public string Name => $"{Method} {Path}";
    }

    public sealed record CaseIds(Guid WalletId, Guid AccountId, Guid OtherMemberId)
    {
        public string Fill(string path) => path
            .Replace("{w}", WalletId.ToString(), StringComparison.Ordinal)
            .Replace("{a}", AccountId.ToString(), StringComparison.Ordinal)
            .Replace("{u}", OtherMemberId.ToString(), StringComparison.Ordinal);
    }
}
