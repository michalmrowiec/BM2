using System.Globalization;
using BM2.Client.Components.Records;
using BM2.ClientTests.Infrastructure;
using BM2.Contracts.Accounts;
using BM2.Contracts.Categories;
using BM2.Contracts.Records;
using BM2.Contracts.Tags;
using BM2.Contracts.Wallets;

namespace BM2.ClientTests.Components;

public class RecordFormTests : BunitContext
{
    private static readonly AccountItem Cash = new(Guid.NewGuid(), "Cash", "PLN", 0, 0, false, true);
    private static readonly AccountItem Savings = new(Guid.NewGuid(), "Savings", "PLN", 0, 0, false, true);
    private static readonly AccountItem Euro = new(Guid.NewGuid(), "Euro", "EUR", 0, 0, false, true);
    private static readonly AccountItem ReadOnly = new(Guid.NewGuid(), "Joint", "PLN", 0, 0, false, false);
    private static readonly CategoryItem Groceries = new(Guid.NewGuid(), "Groceries", CategoryType.Expense, false);
    private static readonly CategoryItem Transport = new(Guid.NewGuid(), "Transport", CategoryType.Expense, false);
    private static readonly CategoryItem Old = new(Guid.NewGuid(), "Old hobby", CategoryType.Expense, true);
    private static readonly CategoryItem Salary = new(Guid.NewGuid(), "Salary", CategoryType.Income, false);
    private static readonly TagItem House = new(Guid.NewGuid(), "House", false);

    private static readonly WalletSnapshot Wallet = Snapshots.Owner(Guid.NewGuid(), accounts: [Cash, Savings, Euro, ReadOnly]) with
    {
        Categories = [Groceries, Transport, Old, Salary],
        Tags = [House],
    };

    private readonly CultureScope _culture = new("en-US");
    private SaveRecordRequest? _submitted;

    public RecordFormTests() => this.UseBm2Client();

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Now);

    private IRenderedComponent<RecordForm> RenderForm(RecordItem? initial = null, bool isCopy = false, Guid? defaultAccount = null) =>
        Render<RecordForm>(p => p
            .Add(x => x.Wallet, Wallet)
            .Add(x => x.Initial, initial)
            .Add(x => x.IsCopy, isCopy)
            .Add(x => x.DefaultAccountId, defaultAccount)
            .Add(x => x.OnSubmit, request => _submitted = request));

    private static void SelectChip(IRenderedComponent<RecordForm> cut, string label) =>
        cut.FindAll(".chip").Single(c => c.TextContent.Trim() == label).Click();

    private static void SelectType(IRenderedComponent<RecordForm> cut, string label) =>
        cut.FindAll("[role=radio]").Single(r => r.TextContent.Trim() == label).Click();

    [Fact]
    public void Expense_shows_only_active_expense_categories()
    {
        var cut = RenderForm();

        cut.FindAll(".chip-group--categories .chip").Select(c => c.TextContent.Trim()).ShouldBe(["Groceries", "Transport"]);
    }

    [Fact]
    public void Switching_type_clears_selected_category()
    {
        var cut = RenderForm();
        SelectChip(cut, "Groceries");

        SelectType(cut, "Income");

        cut.FindAll(".chip-group--categories .chip").Select(c => c.TextContent.Trim()).ShouldBe(["Salary"]);
        cut.FindAll(".chip-group--categories [aria-pressed=true]").ShouldBeEmpty();
    }

    [Fact]
    public void Defaults_to_last_used_account_and_today()
    {
        var cut = RenderForm(defaultAccount: Savings.Id);

        cut.Find("#record-account").GetAttribute("value").ShouldBe(Savings.Id.ToString());
        cut.Find("#record-date").GetAttribute("value").ShouldBe(Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Only_writable_active_accounts_are_offered()
    {
        var cut = RenderForm();

        cut.FindAll("#record-account option").Select(o => o.TextContent).ShouldBe(["Cash", "Savings", "Euro"]);
    }

    [Fact]
    public void Foreign_currency_reveals_rate_and_converts()
    {
        var cut = RenderForm();
        cut.FindAll("#record-rate").ShouldBeEmpty();

        cut.Find("#record-currency").Change("EUR");
        cut.Find("#record-amount").Change("10");
        cut.Find("#record-rate").Change("4.3125");

        cut.Find("#record-account-amount").GetAttribute("value").ShouldBe("43.13");
    }

    [Fact]
    public void Account_amount_updates_the_rate()
    {
        var cut = RenderForm();
        cut.Find("#record-currency").Change("EUR");
        cut.Find("#record-amount").Change("10");

        cut.Find("#record-account-amount").Change("43.17");

        cut.Find("#record-rate").GetAttribute("value").ShouldBe("4.317");
    }

    [Fact]
    public void Transfer_hides_categories_and_lists_other_accounts_as_target()
    {
        var cut = RenderForm(defaultAccount: Cash.Id);

        SelectType(cut, "Transfer");

        cut.FindAll(".chip-group--categories").ShouldBeEmpty();
        cut.FindAll("#record-target option").Select(o => o.TextContent).ShouldBe(["Savings", "Euro"]);
    }

    [Fact]
    public void Transfer_between_currencies_asks_for_received_amount()
    {
        var cut = RenderForm(defaultAccount: Cash.Id);
        SelectType(cut, "Transfer");

        cut.Find("#record-target").Change(Euro.Id.ToString());

        cut.FindAll("#record-transfer-amount").ShouldHaveSingleItem();
    }

    [Fact]
    public void Submits_request_with_selected_values()
    {
        var cut = RenderForm(defaultAccount: Cash.Id);
        cut.Find("#record-amount").Change("12.5");
        SelectChip(cut, "Groceries");

        cut.Find("form").Submit();

        _submitted.ShouldNotBeNull();
        _submitted.TagIds.ShouldBeEmpty();
        _submitted.ShouldBe(new SaveRecordRequest(
            RecordType.Expense, RecordStatus.Completed, Today, Cash.Id, 12.5m, "PLN", 12.5m, Groceries.Id, null, null, null,
            _submitted.TagIds));
    }

    [Fact]
    public void Copy_keeps_fields_but_uses_today_and_completed()
    {
        var source = new RecordItem(
            Guid.NewGuid(), RecordType.Expense, RecordStatus.Planned, new DateOnly(2026, 1, 5), Savings.Id, "Savings", 8m, "PLN",
            8m, Transport.Id, null, null, null, "Bus", [House.Id]);
        var cut = RenderForm(source, isCopy: true);

        cut.Find("form").Submit();

        _submitted!.OccurredOn.ShouldBe(Today);
        _submitted.Status.ShouldBe(RecordStatus.Completed);
        _submitted.CategoryId.ShouldBe(Transport.Id);
        _submitted.AccountId.ShouldBe(Savings.Id);
        _submitted.Name.ShouldBe("Bus");
        _submitted.TagIds.ShouldBe([House.Id]);
    }

    [Fact]
    public void Editing_keeps_archived_category_of_the_record()
    {
        var source = new RecordItem(
            Guid.NewGuid(), RecordType.Expense, RecordStatus.Completed, new DateOnly(2026, 1, 5), Cash.Id, "Cash", 8m, "PLN",
            8m, Old.Id, null, null, null, null, []);

        var cut = RenderForm(source);

        cut.Find(".chip-group--categories [aria-pressed=true]").TextContent.Trim().ShouldBe("Old hobby");
    }

    [Fact]
    public void Missing_amount_shows_error_and_does_not_submit()
    {
        var cut = RenderForm();
        SelectChip(cut, "Groceries");

        cut.Find("form").Submit();

        cut.Find("#record-amount-error").TextContent.ShouldNotBeEmpty();
        _submitted.ShouldBeNull();
    }

    [Fact]
    public void Missing_category_shows_error()
    {
        var cut = RenderForm();
        cut.Find("#record-amount").Change("5");

        cut.Find("form").Submit();

        cut.Find(".record-form__categories .field__error").TextContent.ShouldBe("Choose a category.");
        _submitted.ShouldBeNull();
    }

    protected override void Dispose(bool disposing)
    {
        _culture.Dispose();
        base.Dispose(disposing);
    }
}
