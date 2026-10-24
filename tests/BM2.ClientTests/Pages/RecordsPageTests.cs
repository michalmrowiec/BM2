using BM2.Client.Pages;
using BM2.ClientTests.Infrastructure;
using BM2.Contracts.Accounts;
using BM2.Contracts.Categories;
using BM2.Contracts.Records;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BM2.ClientTests.Pages;

public class RecordsPageTests : BunitContext
{
    private static readonly Guid WalletId = Guid.NewGuid();
    private static readonly AccountItem Cash = new(Guid.NewGuid(), "Cash", "PLN", 0, 0, false, true);
    private static readonly CategoryItem Groceries = new(Guid.NewGuid(), "Groceries", CategoryType.Expense, false);
    private readonly StubApi _api;

    public RecordsPageTests()
    {
        AddAuthorization().SetAuthorized("me@example.com");
        _api = this.UseBm2Client().WithWallets(Snapshots.Owner(WalletId, accounts: [Cash]) with { Categories = [Groceries] });
    }

    private static RecordItem Item(string name, DateOnly day) => new(
        Guid.NewGuid(), RecordType.Expense, RecordStatus.Completed, day, Cash.Id, "Cash", 1m, "PLN", 1m, Groceries.Id, null,
        null, null, name, []);

    private void Month(string month, string from, string to, Task<HttpResponseMessage> page)
    {
        var query = $"?from={from}&to={to}";
        _api.OnAsync(HttpMethod.Get, $"api/wallets/{WalletId}/records{query}", _ => page)
            .OnGet($"api/wallets/{WalletId}/records/summary{query}", new RecordSummary([]));
        _ = month;
    }

    [Fact]
    public void Slow_response_for_an_old_month_does_not_replace_the_new_month()
    {
        var october = new TaskCompletionSource<HttpResponseMessage>();
        Month("2026-10", "2026-10-01", "2026-11-01", october.Task);
        Month("2026-09", "2026-09-01", "2026-10-01",
            Task.FromResult(StubApi.Json(new RecordPage([Item("September bread", new DateOnly(2026, 9, 3))], null))));
        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo("records?month=2026-10");
        var cut = Render<Records>();

        navigation.NavigateTo("records?month=2026-09");
        cut.WaitForAssertion(() => _api.Requests.ShouldContain(r => r.Contains("from=2026-09-01")));
        cut.WaitForAssertion(() => cut.Markup.ShouldContain("September bread"), TimeSpan.FromSeconds(5));
        october.SetResult(StubApi.Json(new RecordPage([Item("October milk", new DateOnly(2026, 10, 3))], null)));

        cut.WaitForAssertion(() => cut.Markup.ShouldNotContain("October milk"), TimeSpan.FromSeconds(2));
        cut.Markup.ShouldContain("September bread");
    }
}
