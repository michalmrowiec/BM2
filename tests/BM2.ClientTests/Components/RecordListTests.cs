using BM2.Client.Components.Records;
using BM2.ClientTests.Infrastructure;
using BM2.Contracts.Accounts;
using BM2.Contracts.Categories;
using BM2.Contracts.Records;
using BM2.Contracts.Tags;
using BM2.Contracts.Wallets;

namespace BM2.ClientTests.Components;

public class RecordListTests : BunitContext
{
    private static readonly AccountItem Cash = new(Guid.NewGuid(), "Cash", "PLN", 0, 0, 0, false, true);
    private static readonly CategoryItem Groceries = new(Guid.NewGuid(), "Groceries", CategoryType.Expense, false);
    private static readonly TagItem House = new(Guid.NewGuid(), "House", false);
    private static readonly DateOnly Day = new(2026, 10, 20);

    private readonly CultureScope _culture = new("en-US");

    private static WalletSnapshot Wallet(params AccountItem[] accounts) =>
        Snapshots.Owner(Guid.NewGuid(), accounts: accounts) with { Categories = [Groceries], Tags = [House] };

    private static RecordItem Expense(string? name = null, RecordStatus status = RecordStatus.Completed) => new(
        Guid.NewGuid(), RecordType.Expense, status, Day, Cash.Id, "Cash", 12.5m, "PLN", 12.5m, Groceries.Id, null, null, null,
        name, [House.Id]);

    private IRenderedComponent<RecordList> RenderList(WalletSnapshot wallet, params RecordItem[] items) =>
        Render<RecordList>(p => p.Add(x => x.Wallet, wallet).Add(x => x.Items, items));

    [Fact]
    public void Expense_shows_minus_and_category_name_when_unnamed()
    {
        var cut = RenderList(Wallet(Cash), Expense());

        var row = cut.Find(".record-row");
        row.TextContent.ShouldContain("Groceries");
        row.TextContent.ShouldContain("−12.50 PLN");
        row.TextContent.ShouldContain("Cash · House");
    }

    [Fact]
    public void Named_record_shows_its_name()
    {
        var cut = RenderList(Wallet(Cash), Expense("Bakery"));

        cut.Find(".record-row__title").TextContent.ShouldBe("Bakery");
    }

    [Fact]
    public void Planned_record_has_text_label()
    {
        var cut = RenderList(Wallet(Cash), Expense(status: RecordStatus.Planned));

        cut.Find(".record-row").TextContent.ShouldContain("Planned");
    }

    [Fact]
    public void Day_header_shows_date_and_day_total()
    {
        var cut = RenderList(Wallet(Cash), Expense(), Expense());

        var header = cut.Find(".record-day__header");
        header.TextContent.ShouldContain("Tuesday, 20 October");
        header.TextContent.ShouldContain("−25.00 PLN");
    }

    [Fact]
    public void Incoming_transfer_for_shared_account_shows_plus_and_counterpart()
    {
        var euro = new AccountItem(Guid.NewGuid(), "Euro", "EUR", 0, 0, 0, false, false);
        var transfer = new RecordItem(
            Guid.NewGuid(), RecordType.Transfer, RecordStatus.Completed, Day, Guid.NewGuid(), "Savings", 430m, "PLN", 430m,
            null, euro.Id, "Euro", 100m, null, []);

        var cut = RenderList(Wallet(euro), transfer);

        var row = cut.Find(".record-row");
        row.TextContent.ShouldContain("+100.00 EUR");
        row.TextContent.ShouldContain("Transfer from Savings");
    }

    [Fact]
    public void Outgoing_transfer_shows_minus_and_target()
    {
        var savings = new AccountItem(Guid.NewGuid(), "Savings", "PLN", 0, 0, 0, false, true);
        var transfer = new RecordItem(
            Guid.NewGuid(), RecordType.Transfer, RecordStatus.Completed, Day, Cash.Id, "Cash", 50m, "PLN", 50m, null, savings.Id,
            "Savings", 50m, null, []);

        var cut = RenderList(Wallet(Cash, savings), transfer);

        cut.Find(".record-row").TextContent.ShouldContain("Transfer to Savings");
        cut.Find(".record-row").TextContent.ShouldContain("−50.00 PLN");
    }

    [Fact]
    public void Row_details_link_to_the_record_editor()
    {
        var expense = Expense();

        var cut = RenderList(Wallet(Cash), expense);
        cut.Find(".record-row__main").Click();

        cut.Find(".record-details__actions a").GetAttribute("href").ShouldBe($"records/{expense.Id}");
    }

    protected override void Dispose(bool disposing)
    {
        _culture.Dispose();
        base.Dispose(disposing);
    }
}
