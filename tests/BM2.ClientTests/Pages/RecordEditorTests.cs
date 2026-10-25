using BM2.Client.Pages;
using BM2.Client.Services;
using BM2.ClientTests.Infrastructure;
using BM2.Contracts.Accounts;
using BM2.Contracts.Categories;
using BM2.Contracts.Records;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BM2.ClientTests.Pages;

public class RecordEditorTests : BunitContext
{
    private static readonly Guid WalletId = Guid.NewGuid();
    private static readonly AccountItem Cash = new(Guid.NewGuid(), "Cash", "PLN", 0, 0, 0, false, true);
    private static readonly CategoryItem Groceries = new(Guid.NewGuid(), "Groceries", CategoryType.Expense, false);

    private readonly CultureScope _culture = new("en-US");

    public RecordEditorTests()
    {
        AddAuthorization().SetAuthorized("me@example.com");
        this.UseBm2Client()
            .WithWallets(Snapshots.Owner(WalletId, accounts: [Cash]) with { Categories = [Groceries] })
            .On(HttpMethod.Post, $"api/wallets/{WalletId}/records", _ => StubApi.Json(new RecordItem(
                Guid.NewGuid(), RecordType.Expense, RecordStatus.Completed, new DateOnly(2026, 10, 24), Cash.Id, "Cash", 5m, "PLN", 5m,
                Groceries.Id, null, null, null, null, [])));
    }

    private BunitNavigationManager Navigation => (BunitNavigationManager)Services.GetRequiredService<NavigationManager>();

    private static void SaveExpense(IRenderedComponent<RecordEditor> cut)
    {
        cut.WaitForElement("#record-amount").Change("5");
        cut.FindAll(".chip").Single(c => c.TextContent.Trim() == "Groceries").Click();
        cut.Find("form").Submit();
    }

    [Fact]
    public void Saving_goes_back_to_where_the_user_came_from()
    {
        _ = Services.GetRequiredService<AppNavigation>();
        Navigation.NavigateTo("records?month=2026-09");
        Navigation.NavigateTo("records/new");
        var cut = Render<RecordEditor>();

        SaveExpense(cut);

        cut.WaitForAssertion(() => JSInterop.VerifyInvoke("history.back"));
    }

    [Fact]
    public void Saving_a_directly_opened_editor_goes_to_records()
    {
        var cut = Render<RecordEditor>();

        SaveExpense(cut);

        cut.WaitForAssertion(() => Navigation.Uri.ShouldBe("http://localhost/records"));
        JSInterop.Invocations.ShouldNotContain(i => i.Identifier == "history.back");
    }

    protected override void Dispose(bool disposing)
    {
        _culture.Dispose();
        base.Dispose(disposing);
    }
}
