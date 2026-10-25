using System.Net;
using System.Net.Http.Json;
using BM2.Contracts.Accounts;
using BM2.Contracts.Categories;
using BM2.Contracts.Invitations;
using BM2.Contracts.Members;
using BM2.Contracts.Records;
using BM2.Contracts.Stats;
using BM2.Contracts.Tags;
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
        new("POST", "/api/wallets/{w}/accounts", _ => new CreateAccountRequest("New", "PLN", 0), MemberRole.Owner),
        new("PUT", "/api/wallets/{w}/accounts/{a}", _ => new UpdateAccountRequest("Renamed", "PLN", 0, false), MemberRole.Owner),
        new("DELETE", "/api/wallets/{w}/accounts/{a}", null, MemberRole.Owner),
        new("POST", "/api/wallets/{w}/categories", _ => new CreateCategoryRequest("New", CategoryType.Expense), MemberRole.Editor),
        new("PUT", "/api/wallets/{w}/categories/{c}", _ => new UpdateCategoryRequest("Renamed", false), MemberRole.Editor),
        new("DELETE", "/api/wallets/{w}/categories/{c}", null, MemberRole.Editor),
        new("POST", "/api/wallets/{w}/tags", _ => new CreateTagRequest("New"), MemberRole.Editor),
        new("PUT", "/api/wallets/{w}/tags/{t}", _ => new UpdateTagRequest("Renamed", false), MemberRole.Editor),
        new("DELETE", "/api/wallets/{w}/tags/{t}", null, MemberRole.Editor),
        new("GET", "/api/wallets/{w}/records", null, MemberRole.Viewer),
        new("GET", "/api/wallets/{w}/records/summary", null, MemberRole.Viewer),
        new("GET", "/api/wallets/{w}/records/{r}", null, MemberRole.Viewer),
        new("GET", "/api/wallets/{w}/stats/categories", null, MemberRole.Viewer),
        new("GET", "/api/wallets/{w}/stats/monthly", null, MemberRole.Viewer),
        new("GET", "/api/wallets/{w}/stats/category-months", null, MemberRole.Viewer),
        new("GET", "/api/wallets/{w}/stats/forecast", null, MemberRole.Viewer),
        new("GET", "/api/wallets/{w}/records/export", null, MemberRole.Viewer),
        new("POST", "/api/wallets/{w}/pinned-charts", _ => new PinChartRequest(PinnedChartKind.Monthly, "Monthly", ""), MemberRole.Viewer),
        new("POST", "/api/wallets/{w}/records", ValidRecord, MemberRole.Editor),
        new("PUT", "/api/wallets/{w}/records/{r}", ValidRecord, MemberRole.Editor),
        new("DELETE", "/api/wallets/{w}/records/{r}", null, MemberRole.Editor),
        new("GET", "/api/wallets/{w}/members", null, MemberRole.Viewer),
        new("PUT", "/api/wallets/{w}/members/{u}", _ => new UpdateRoleRequest(MemberRole.Viewer), MemberRole.Owner),
        new("PUT", "/api/wallets/{w}/accounts/{a}/members/{u}", _ => new UpdateRoleRequest(MemberRole.Viewer), MemberRole.Owner),
        new("DELETE", "/api/wallets/{w}/members/{u}", null, MemberRole.Owner),
        new("DELETE", "/api/wallets/{w}/accounts/{a}/members/{u}", null, MemberRole.Owner),
        new("POST", "/api/wallets/{w}/invitations", _ => new CreateInvitationRequest(MemberRole.Viewer, null), MemberRole.Owner),
        new("DELETE", "/api/wallets/{w}/invitations/{i}", null, MemberRole.Owner),
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

    private static SaveRecordRequest ValidRecord(CaseIds ids) => new SaveRecordRequest(
        RecordType.Expense, RecordStatus.Completed, new DateOnly(2026, 10, 20), ids.AccountId, 5m, "PLN", 5m, ids.CategoryId,
        null, null, null, []);

    private async Task<CaseIds> SeedAsync()
    {
        var owner = await CreateUserAsync("owner@example.com");
        var other = await CreateUserAsync("editor@example.com");
        var wallet = await Data.WalletAsync(owner.Id);
        await Data.MemberAsync(wallet.Id, other.Id, MemberRole.Editor);
        var account = await Data.AccountAsync(wallet.Id);
        var category = await Data.CategoryAsync(wallet.Id);
        var tag = await Data.TagAsync(wallet.Id);
        var record = await Data.RecordAsync(account.Id, 10m, categoryId: category.Id);
        var (invitation, _) = await Data.InvitationAsync(wallet.Id, owner.Id);
        return new CaseIds(wallet.Id, account.Id, other.Id, category.Id, tag.Id, record.Id, invitation.Id);
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

    public sealed record CaseIds(
        Guid WalletId, Guid AccountId, Guid OtherMemberId, Guid CategoryId, Guid TagId, Guid RecordId, Guid InvitationId)
    {
        public string Fill(string path) => path
            .Replace("{w}", WalletId.ToString(), StringComparison.Ordinal)
            .Replace("{a}", AccountId.ToString(), StringComparison.Ordinal)
            .Replace("{u}", OtherMemberId.ToString(), StringComparison.Ordinal)
            .Replace("{c}", CategoryId.ToString(), StringComparison.Ordinal)
            .Replace("{t}", TagId.ToString(), StringComparison.Ordinal)
            .Replace("{r}", RecordId.ToString(), StringComparison.Ordinal)
            .Replace("{i}", InvitationId.ToString(), StringComparison.Ordinal);
    }
}
