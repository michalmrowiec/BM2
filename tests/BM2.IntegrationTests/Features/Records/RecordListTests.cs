using System.Net;
using System.Net.Http.Json;
using BM2.Contracts.Categories;
using BM2.Contracts.Records;
using BM2.Contracts.Wallets;
using BM2.Domain.Records;
using BM2.IntegrationTests.Infrastructure;
using Record = BM2.Domain.Records.Record;

namespace BM2.IntegrationTests.Features.Records;

public class RecordListTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Month_range_includes_last_day_and_excludes_next_month()
    {
        var s = await SetupAsync();
        foreach (var day in new[] { new DateOnly(2026, 9, 30), new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31), new DateOnly(2026, 11, 1) })
        {
            await Data.RecordAsync(s.Cash, 1m, categoryId: s.Groceries, occurredOn: day);
        }

        var page = await ListAsync(s, new RecordFilter { From = new DateOnly(2026, 10, 1), To = new DateOnly(2026, 11, 1) });

        page.Items.Select(i => i.OccurredOn).ShouldBe([new DateOnly(2026, 10, 31), new DateOnly(2026, 10, 1)]);
        page.NextCursor.ShouldBeNull();
    }

    [Fact]
    public async Task Paging_returns_every_record_once_in_order()
    {
        var s = await SetupAsync();
        await SeedManyAsync(s, 120);

        var all = new List<RecordItem>();
        string? cursor = null;
        do
        {
            var page = await ListAsync(s, new RecordFilter { Cursor = cursor });
            page.Items.Count.ShouldBeLessThanOrEqualTo(50);
            all.AddRange(page.Items);
            cursor = page.NextCursor;
        }
        while (cursor is not null);

        all.Count.ShouldBe(120);
        all.Select(i => i.Id).Distinct().Count().ShouldBe(120);
        all.Select(i => i.OccurredOn).ShouldBe(all.Select(i => i.OccurredOn).OrderByDescending(d => d));
    }

    [Fact]
    public async Task Limit_caps_the_page_size()
    {
        var s = await SetupAsync();
        await SeedManyAsync(s, 8);

        var page = await ListAsync(s, new RecordFilter { Limit = 5 });

        page.Items.Count.ShouldBe(5);
        page.NextCursor.ShouldNotBeNull();
    }

    [Fact]
    public async Task Tag_filter_without_dates_spans_all_time()
    {
        var s = await SetupAsync();
        var house = await Data.TagAsync(s.Wallet, "House");
        foreach (var year in new[] { 2024, 2025, 2026 })
        {
            await Data.RecordAsync(s.Cash, 100m, categoryId: s.Groceries, occurredOn: new DateOnly(year, 5, 1), tagIds: [house.Id]);
        }

        await Data.RecordAsync(s.Cash, 5m, categoryId: s.Groceries);

        var page = await ListAsync(s, new RecordFilter { TagId = house.Id });
        var summary = await SummaryAsync(s, new RecordFilter { TagId = house.Id });

        page.Items.Count.ShouldBe(3);
        summary.Totals.ShouldBe([new CurrencyTotals("PLN", 0m, 300m)]);
    }

    [Fact]
    public async Task Account_filter_includes_incoming_transfers()
    {
        var s = await SetupAsync();
        var savings = await Data.AccountAsync(s.Wallet, "Savings");
        await Data.RecordAsync(s.Cash, 50m, RecordType.Transfer, transferAccountId: savings.Id);
        await Data.RecordAsync(s.Cash, 5m, categoryId: s.Groceries);

        var page = await ListAsync(s, new RecordFilter { AccountId = savings.Id });

        page.Items.ShouldHaveSingleItem().TransferAccountId.ShouldBe(savings.Id);
    }

    [Fact]
    public async Task Search_matches_name_fragment()
    {
        var s = await SetupAsync();
        await Data.RecordAsync(s.Cash, 5m, categoryId: s.Groceries, name: "Bakery Lidl");
        await Data.RecordAsync(s.Cash, 5m, categoryId: s.Groceries, name: "Fuel");

        var page = await ListAsync(s, new RecordFilter { Search = "lidl" });

        page.Items.ShouldHaveSingleItem().Name.ShouldBe("Bakery Lidl");
    }

    [Fact]
    public async Task Search_matches_category_name_of_unnamed_records()
    {
        var s = await SetupAsync();
        var transport = await Data.CategoryAsync(s.Wallet, "Transport");
        await Data.RecordAsync(s.Cash, 5m, categoryId: transport.Id);
        await Data.RecordAsync(s.Cash, 5m, categoryId: s.Groceries);

        var page = await ListAsync(s, new RecordFilter { Search = "trans" });

        page.Items.ShouldHaveSingleItem().CategoryId.ShouldBe(transport.Id);
    }

    [Fact]
    public async Task Type_and_status_filters_narrow_the_list()
    {
        var s = await SetupAsync();
        var salary = await Data.CategoryAsync(s.Wallet, "Salary", CategoryType.Income);
        await Data.RecordAsync(s.Cash, 5m, categoryId: s.Groceries, status: RecordStatus.Planned);
        await Data.RecordAsync(s.Cash, 6m, categoryId: s.Groceries);
        await Data.RecordAsync(s.Cash, 7m, RecordType.Income, salary.Id);

        var planned = await ListAsync(s, new RecordFilter { Status = RecordStatus.Planned });
        var income = await ListAsync(s, new RecordFilter { Type = RecordType.Income });

        planned.Items.ShouldHaveSingleItem().Amount.ShouldBe(5m);
        income.Items.ShouldHaveSingleItem().Amount.ShouldBe(7m);
    }

    [Fact]
    public async Task Shared_account_user_lists_only_its_records()
    {
        var s = await SetupAsync();
        var savings = await Data.AccountAsync(s.Wallet, "Savings");
        await Data.RecordAsync(s.Cash, 5m, categoryId: s.Groceries);
        await Data.RecordAsync(savings.Id, 6m, categoryId: s.Groceries);
        await Data.RecordAsync(savings.Id, 7m, RecordType.Transfer, transferAccountId: s.Cash);
        var friend = await CreateUserAsync("friend@example.com");
        await Data.ShareAsync(s.Cash, friend.Id, MemberRole.Viewer);
        using var client = Factory.CreateClientAs(friend.Id);

        var page = await client.GetFromJsonAsync<RecordPage>($"/api/wallets/{s.Wallet}/records", Ct);

        page!.Items.Count.ShouldBe(2);
        page.Items.ShouldAllBe(i => i.AccountId == s.Cash || i.TransferAccountId == s.Cash);
    }

    [Fact]
    public async Task Unreadable_account_filter_returns_404()
    {
        var s = await SetupAsync();
        var savings = await Data.AccountAsync(s.Wallet, "Savings");
        var friend = await CreateUserAsync("friend@example.com");
        await Data.ShareAsync(s.Cash, friend.Id, MemberRole.Viewer);
        using var client = Factory.CreateClientAs(friend.Id);

        var response = await client.GetAsync($"/api/wallets/{s.Wallet}/records?accountId={savings.Id}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Invalid_cursor_returns_400()
    {
        var s = await SetupAsync();

        var response = await s.Client.GetAsync($"/api/wallets/{s.Wallet}/records?cursor=not-a-cursor", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Summary_sums_per_account_currency_without_transfers()
    {
        var s = await SetupAsync();
        var euro = await Data.AccountAsync(s.Wallet, "Euro", "EUR");
        var salary = await Data.CategoryAsync(s.Wallet, "Salary", CategoryType.Income);
        await Data.RecordAsync(s.Cash, 5000m, RecordType.Income, salary.Id);
        await Data.RecordAsync(s.Cash, 300m, categoryId: s.Groceries);
        await Data.RecordAsync(s.Cash, 12.5m, categoryId: s.Groceries);
        await Data.RecordAsync(euro.Id, 20m, categoryId: s.Groceries);
        await Data.RecordAsync(s.Cash, 100m, RecordType.Transfer, transferAccountId: euro.Id, transferAmount: 23m);

        var summary = await SummaryAsync(s, new RecordFilter());

        summary.Totals.ShouldBe([new CurrencyTotals("EUR", 0m, 20m), new CurrencyTotals("PLN", 5000m, 312.5m)]);
    }

    [Fact]
    public async Task Summary_of_shared_account_user_ignores_other_accounts()
    {
        var s = await SetupAsync();
        var savings = await Data.AccountAsync(s.Wallet, "Savings");
        await Data.RecordAsync(s.Cash, 12.5m, categoryId: s.Groceries);
        await Data.RecordAsync(savings.Id, 99m, categoryId: s.Groceries);
        var friend = await CreateUserAsync("friend@example.com");
        await Data.ShareAsync(s.Cash, friend.Id, MemberRole.Viewer);
        using var client = Factory.CreateClientAs(friend.Id);

        var summary = await client.GetFromJsonAsync<RecordSummary>($"/api/wallets/{s.Wallet}/records/summary", Ct);

        summary!.Totals.ShouldHaveSingleItem().Expense.ShouldBe(12.5m);
    }

    private async Task SeedManyAsync(Setup s, int count)
    {
        await using var db = Fixture.CreateDbContext();
        var account = new AccountRef(s.Cash, s.Wallet, "PLN", false);
        var category = new CategoryRef(s.Groceries, s.Wallet, CategoryType.Expense, false);
        var createdAt = DateTimeOffset.UtcNow;
        for (var i = 0; i < count; i++)
        {
            // Ten records per day, all with the same timestamp: the id breaks ties.
            var draft = new RecordDraft(
                RecordType.Expense, RecordStatus.Completed, new DateOnly(2026, 10, 1).AddDays(i / 10), s.Cash, 1m + i, "PLN",
                1m + i, s.Groceries, null, null, null, []);
            db.Records.Add(Record.Create(draft, new RecordContext(s.Wallet, account, null, category, []), s.UserId, createdAt).Value);
        }

        await db.SaveChangesAsync(Ct);
    }

    private static async Task<RecordPage> ListAsync(Setup s, RecordFilter filter) =>
        (await s.Client.GetFromJsonAsync<RecordPage>($"/api/wallets/{s.Wallet}/records{filter.ToQueryString()}", Ct))!;

    private static async Task<RecordSummary> SummaryAsync(Setup s, RecordFilter filter) =>
        (await s.Client.GetFromJsonAsync<RecordSummary>($"/api/wallets/{s.Wallet}/records/summary{filter.ToQueryString()}", Ct))!;

    private async Task<Setup> SetupAsync()
    {
        var owner = await CreateUserAsync("owner@example.com");
        var wallet = await Data.WalletAsync(owner.Id);
        var cash = await Data.AccountAsync(wallet.Id, "Cash");
        var groceries = await Data.CategoryAsync(wallet.Id);
        return new Setup(Factory.CreateClientAs(owner.Id), owner.Id, wallet.Id, cash.Id, groceries.Id);
    }

    private sealed record Setup(HttpClient Client, Guid UserId, Guid Wallet, Guid Cash, Guid Groceries);
}
