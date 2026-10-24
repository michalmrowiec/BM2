using System.Net;
using System.Net.Http.Json;
using BM2.Contracts.Accounts;
using BM2.Contracts.Categories;
using BM2.Contracts.Records;
using BM2.Contracts.Wallets;
using BM2.Domain.Records;
using BM2.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BM2.IntegrationTests.Features.Records;

public class RecordTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    private static readonly DateOnly Day = new(2026, 10, 20);

    [Fact]
    public async Task Editor_adds_expense_and_balance_drops()
    {
        var s = await SetupAsync(MemberRole.Editor, openingBalance: 100m);

        var response = await s.Client.PostAsJsonAsync(s.Records, Expense(s, 12.5m), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        (await BalanceAsync(s, s.Account)).ShouldBe(87.5m);
    }

    [Fact]
    public async Task Created_record_is_returned_with_names()
    {
        var s = await SetupAsync(MemberRole.Owner);

        var item = await (await s.Client.PostAsJsonAsync(s.Records, Expense(s, 12.5m) with { Name = " Bakery " }, Ct))
            .Content.ReadFromJsonAsync<RecordItem>(Ct);

        item!.AccountName.ShouldBe("Cash");
        item.Name.ShouldBe("Bakery");
        item.CategoryId.ShouldBe(s.Category);
    }

    [Fact]
    public async Task Planned_record_does_not_change_balance()
    {
        var s = await SetupAsync(MemberRole.Owner, openingBalance: 100m);

        await s.Client.PostAsJsonAsync(s.Records, Expense(s, 12.5m) with { Status = RecordStatus.Planned }, Ct);

        (await BalanceAsync(s, s.Account)).ShouldBe(100m);
    }

    [Fact]
    public async Task Income_raises_balance()
    {
        var s = await SetupAsync(MemberRole.Owner);
        var salary = await Data.CategoryAsync(s.Wallet, "Salary", CategoryType.Income);

        await s.Client.PostAsJsonAsync(s.Records, Expense(s, 5000m) with { Type = RecordType.Income, CategoryId = salary.Id }, Ct);

        (await BalanceAsync(s, s.Account)).ShouldBe(5000m);
    }

    [Fact]
    public async Task Transfer_moves_money_between_currencies()
    {
        var s = await SetupAsync(MemberRole.Owner);
        var euro = await Data.AccountAsync(s.Wallet, "Euro", "EUR");

        var response = await s.Client.PostAsJsonAsync(s.Records, Transfer(s, s.Account, euro.Id, 430m) with { TransferAmount = 100m }, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        (await BalanceAsync(s, s.Account)).ShouldBe(-430m);
        (await BalanceAsync(s, euro.Id)).ShouldBe(100m);
    }

    [Fact]
    public async Task Foreign_currency_expense_uses_account_amount_for_balance()
    {
        var s = await SetupAsync(MemberRole.Owner);

        await s.Client.PostAsJsonAsync(s.Records, Expense(s, 10m) with { Currency = "EUR", AccountAmount = 43.17m }, Ct);

        (await BalanceAsync(s, s.Account)).ShouldBe(-43.17m);
    }

    [Fact]
    public async Task Viewer_cannot_add_record()
    {
        var s = await SetupAsync(MemberRole.Viewer);

        var response = await s.Client.PostAsJsonAsync(s.Records, Expense(s, 1m), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Shared_account_editor_adds_record_on_shared_account()
    {
        var s = await SharedSetupAsync(MemberRole.Editor);

        var response = await s.Client.PostAsJsonAsync(s.Records, Expense(s, 3m), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Shared_account_editor_cannot_read_record_of_other_account()
    {
        var s = await SharedSetupAsync(MemberRole.Editor);
        var hidden = await Data.RecordAsync(s.Other, 9m, categoryId: s.Category);

        var response = await s.Client.GetAsync($"{s.Records}/{hidden.Id}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Shared_account_editor_cannot_move_record_to_other_account()
    {
        var s = await SharedSetupAsync(MemberRole.Editor);
        var record = await Data.RecordAsync(s.Account, 9m, categoryId: s.Category);

        var response = await s.Client.PutAsJsonAsync($"{s.Records}/{record.Id}", Expense(s, 9m) with { AccountId = s.Other }, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        await using var db = Fixture.CreateDbContext();
        (await db.Records.SingleAsync(r => r.Id == record.Id, Ct)).AccountId.ShouldBe(s.Account);
    }

    [Fact]
    public async Task Shared_account_editor_cannot_edit_transfer_into_unshared_account()
    {
        var s = await SharedSetupAsync(MemberRole.Editor);
        var transfer = await Data.RecordAsync(s.Other, 20m, RecordType.Transfer, transferAccountId: s.Account);

        var response = await s.Client.PutAsJsonAsync($"{s.Records}/{transfer.Id}", Transfer(s, s.Other, s.Account, 1m), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Shared_account_editor_cannot_create_transfer_to_unshared_account()
    {
        var s = await SharedSetupAsync(MemberRole.Editor);

        var response = await s.Client.PostAsJsonAsync(s.Records, Transfer(s, s.Account, s.Other, 5m), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Shared_account_viewer_sees_transfer_counterpart_name()
    {
        var s = await SharedSetupAsync(MemberRole.Viewer);
        var transfer = await Data.RecordAsync(s.Other, 20m, RecordType.Transfer, transferAccountId: s.Account);

        var item = await s.Client.GetFromJsonAsync<RecordItem>($"{s.Records}/{transfer.Id}", Ct);

        item!.AccountName.ShouldBe("Savings");
        item.TransferAccountName.ShouldBe("Cash");
    }

    [Fact]
    public async Task Category_from_other_wallet_returns_404()
    {
        var s = await SetupAsync(MemberRole.Owner);
        var stranger = await CreateUserAsync("stranger@example.com");
        var foreignWallet = await Data.WalletAsync(stranger.Id);
        var foreign = await Data.CategoryAsync(foreignWallet.Id);

        var response = await s.Client.PostAsJsonAsync(s.Records, Expense(s, 1m) with { CategoryId = foreign.Id }, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Account_from_other_wallet_returns_404()
    {
        var s = await SetupAsync(MemberRole.Owner);
        var stranger = await CreateUserAsync("stranger@example.com");
        var foreignWallet = await Data.WalletAsync(stranger.Id);
        var foreign = await Data.AccountAsync(foreignWallet.Id);

        var response = await s.Client.PostAsJsonAsync(s.Records, Expense(s, 1m) with { AccountId = foreign.Id }, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Wrong_category_type_returns_validation_problem()
    {
        var s = await SetupAsync(MemberRole.Owner);

        var response = await s.Client.PostAsJsonAsync(s.Records, Expense(s, 1m) with { Type = RecordType.Income }, Ct);
        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem!.Errors.ShouldContainKey("CategoryId");
    }

    [Fact]
    public async Task Invalid_amount_returns_validation_problem()
    {
        var s = await SetupAsync(MemberRole.Owner);

        var response = await s.Client.PostAsJsonAsync(s.Records, Expense(s, 0m), Ct);
        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem!.Errors.ShouldContainKey("Amount");
    }

    [Fact]
    public async Task Editor_updates_and_deletes_record()
    {
        var s = await SetupAsync(MemberRole.Editor, openingBalance: 100m);
        var record = await Data.RecordAsync(s.Account, 10m, categoryId: s.Category);

        var update = await s.Client.PutAsJsonAsync($"{s.Records}/{record.Id}", Expense(s, 30m), Ct);
        var afterUpdate = await BalanceAsync(s, s.Account);
        var delete = await s.Client.DeleteAsync($"{s.Records}/{record.Id}", Ct);

        update.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        afterUpdate.ShouldBe(70m);
        delete.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await BalanceAsync(s, s.Account)).ShouldBe(100m);
    }

    [Fact]
    public async Task Deleting_category_with_records_returns_409()
    {
        var s = await SetupAsync(MemberRole.Owner);
        await Data.RecordAsync(s.Account, 10m, categoryId: s.Category);

        var response = await s.Client.DeleteAsync($"/api/wallets/{s.Wallet}/categories/{s.Category}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Deleting_account_removes_transfers_and_updates_other_balance()
    {
        var s = await SetupAsync(MemberRole.Owner);
        var savings = await Data.AccountAsync(s.Wallet, "Savings");
        await Data.RecordAsync(s.Account, 50m, RecordType.Transfer, transferAccountId: savings.Id);

        var response = await s.Client.DeleteAsync($"/api/wallets/{s.Wallet}/accounts/{s.Account}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await BalanceAsync(s, savings.Id)).ShouldBe(0m);
        await using var db = Fixture.CreateDbContext();
        (await db.Records.CountAsync(Ct)).ShouldBe(0);
    }

    [Fact]
    public async Task Account_currency_cannot_change_once_it_has_records()
    {
        var s = await SetupAsync(MemberRole.Owner);
        await Data.RecordAsync(s.Account, 10m, categoryId: s.Category);

        var response = await s.Client.PutAsJsonAsync(
            $"/api/wallets/{s.Wallet}/accounts/{s.Account}", new UpdateAccountRequest("Cash", "EUR", 0, false), Ct);
        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(Ct);

        problem!.Errors.ShouldContainKey("Currency");
    }

    [Fact]
    public async Task Deleting_tag_keeps_record()
    {
        var s = await SetupAsync(MemberRole.Owner);
        var tag = await Data.TagAsync(s.Wallet);
        await Data.RecordAsync(s.Account, 10m, categoryId: s.Category, tagIds: [tag.Id]);

        await s.Client.DeleteAsync($"/api/wallets/{s.Wallet}/tags/{tag.Id}", Ct);

        await using var db = Fixture.CreateDbContext();
        (await db.Records.CountAsync(Ct)).ShouldBe(1);
        (await db.Set<RecordTag>().CountAsync(Ct)).ShouldBe(0);
    }

    [Fact]
    public async Task Created_record_stores_author_and_utc_timestamp()
    {
        var s = await SetupAsync(MemberRole.Owner);

        var item = await (await s.Client.PostAsJsonAsync(s.Records, Expense(s, 1m), Ct)).Content.ReadFromJsonAsync<RecordItem>(Ct);

        await using var db = Fixture.CreateDbContext();
        var record = await db.Records.SingleAsync(r => r.Id == item!.Id, Ct);
        record.CreatedByUserId.ShouldBe(s.UserId);
        record.CreatedAt.Offset.ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    public async Task Deleting_wallet_removes_records_and_tags()
    {
        var s = await SetupAsync(MemberRole.Owner);
        var tag = await Data.TagAsync(s.Wallet);
        await Data.RecordAsync(s.Account, 10m, categoryId: s.Category, tagIds: [tag.Id]);

        var response = await s.Client.DeleteAsync($"/api/wallets/{s.Wallet}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        await using var db = Fixture.CreateDbContext();
        (await db.Records.CountAsync(Ct)).ShouldBe(0);
        (await db.Categories.CountAsync(Ct)).ShouldBe(0);
    }

    private static SaveRecordRequest Expense(Setup s, decimal amount) => new(
        RecordType.Expense, RecordStatus.Completed, Day, s.Account, amount, "PLN", amount, s.Category, null, null, null, []);

    private static SaveRecordRequest Transfer(Setup s, Guid from, Guid to, decimal amount) => new(
        RecordType.Transfer, RecordStatus.Completed, Day, from, amount, "PLN", amount, null, to, null, null, []);

    private static async Task<decimal> BalanceAsync(Setup s, Guid accountId)
    {
        var snapshot = await s.OwnerClient.GetFromJsonAsync<WalletSnapshot>($"/api/wallets/{s.Wallet}", Ct);
        return snapshot!.Accounts.Single(a => a.Id == accountId).Balance;
    }

    /// <summary>Owner's wallet with a PLN "Cash" account and an expense category; the client acts with the given role.</summary>
    private async Task<Setup> SetupAsync(MemberRole role, decimal openingBalance = 0)
    {
        var owner = await CreateUserAsync("owner@example.com");
        var wallet = await Data.WalletAsync(owner.Id);
        var account = await Data.AccountAsync(wallet.Id, "Cash", openingBalance: openingBalance);
        var category = await Data.CategoryAsync(wallet.Id);
        var ownerClient = Factory.CreateClientAs(owner.Id);
        if (role == MemberRole.Owner)
        {
            return new Setup(ownerClient, ownerClient, owner.Id, wallet.Id, account.Id, Guid.Empty, category.Id);
        }

        var member = await CreateUserAsync("member@example.com");
        await Data.MemberAsync(wallet.Id, member.Id, role);
        return new Setup(Factory.CreateClientAs(member.Id), ownerClient, member.Id, wallet.Id, account.Id, Guid.Empty, category.Id);
    }

    /// <summary>Like <see cref="SetupAsync"/>, but the client only has "Cash" shared with the given role; "Savings" stays private.</summary>
    private async Task<Setup> SharedSetupAsync(MemberRole role)
    {
        var owner = await SetupAsync(MemberRole.Owner);
        var savings = await Data.AccountAsync(owner.Wallet, "Savings");
        var friend = await CreateUserAsync("friend@example.com");
        await Data.ShareAsync(owner.Account, friend.Id, role);
        return owner with { Client = Factory.CreateClientAs(friend.Id), UserId = friend.Id, Other = savings.Id };
    }

    private sealed record Setup(
        HttpClient Client, HttpClient OwnerClient, Guid UserId, Guid Wallet, Guid Account, Guid Other, Guid Category)
    {
        public string Records => $"/api/wallets/{Wallet}/records";
    }
}
