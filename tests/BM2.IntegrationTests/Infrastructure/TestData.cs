using BM2.Contracts.Categories;
using BM2.Contracts.Records;
using BM2.Contracts.Wallets;
using BM2.Domain.Accounts;
using BM2.Domain.Categories;
using BM2.Domain.Records;
using Record = BM2.Domain.Records.Record;
using BM2.Domain.Tags;
using BM2.Domain.Wallets;
using BM2.Server.Features.Records;
using Microsoft.EntityFrameworkCore;

namespace BM2.IntegrationTests.Infrastructure;

/// <summary>Seeds domain data straight into the test database.</summary>
public sealed class TestData(IntegrationFixture fixture)
{
    public async Task<Wallet> WalletAsync(Guid ownerId, string name = "Home", string baseCurrency = "PLN")
    {
        await using var db = fixture.CreateDbContext();
        var wallet = Wallet.Create(name, baseCurrency, ownerId, DateTimeOffset.UtcNow);
        db.Wallets.Add(wallet);
        await db.SaveChangesAsync();
        return wallet;
    }

    public async Task MemberAsync(Guid walletId, Guid userId, MemberRole role)
    {
        await using var db = fixture.CreateDbContext();
        var wallet = await db.Wallets.Include(w => w.Members).SingleAsync(w => w.Id == walletId);
        wallet.Grant(userId, role, DateTimeOffset.UtcNow);
        await db.SaveChangesAsync();
    }

    public async Task<Account> AccountAsync(Guid walletId, string name = "Cash", string currency = "PLN", decimal openingBalance = 0)
    {
        await using var db = fixture.CreateDbContext();
        var account = Account.Create(walletId, name, currency, openingBalance, DateTimeOffset.UtcNow);
        db.Accounts.Add(account);
        await db.SaveChangesAsync();
        return account;
    }

    public async Task ShareAsync(Guid accountId, Guid userId, MemberRole role)
    {
        await using var db = fixture.CreateDbContext();
        var account = await db.Accounts.Include(a => a.Members).SingleAsync(a => a.Id == accountId);
        account.Grant(userId, role, DateTimeOffset.UtcNow).IsSuccess.ShouldBeTrue();
        await db.SaveChangesAsync();
    }

    public async Task<Category> CategoryAsync(Guid walletId, string name = "Groceries", CategoryType type = CategoryType.Expense)
    {
        await using var db = fixture.CreateDbContext();
        var category = Category.Create(walletId, name, type);
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return category;
    }

    public async Task<Tag> TagAsync(Guid walletId, string name = "House")
    {
        await using var db = fixture.CreateDbContext();
        var tag = Tag.Create(walletId, name);
        db.Tags.Add(tag);
        await db.SaveChangesAsync();
        return tag;
    }

    /// <summary>Completed PLN-style record in the account's currency; finds or creates a category of the right type.</summary>
    public async Task<Record> RecordAsync(
        Guid accountId,
        decimal amount,
        RecordType type = RecordType.Expense,
        Guid? categoryId = null,
        DateOnly? occurredOn = null,
        RecordStatus status = RecordStatus.Completed,
        Guid? transferAccountId = null,
        decimal? transferAmount = null,
        Guid[]? tagIds = null,
        string? name = null,
        Guid? createdBy = null)
    {
        await using var db = fixture.CreateDbContext();
        var account = await db.Accounts.AsNoTracking().SingleAsync(a => a.Id == accountId);
        if (type != RecordType.Transfer && categoryId is null)
        {
            var categoryType = type == RecordType.Income ? CategoryType.Income : CategoryType.Expense;
            var category = await db.Categories.FirstOrDefaultAsync(c => c.WalletId == account.WalletId && c.Type == categoryType)
                ?? Category.Create(account.WalletId, categoryType == CategoryType.Income ? "Salary" : "Groceries", categoryType);
            if (category.Id == Guid.Empty)
            {
                db.Categories.Add(category);
                await db.SaveChangesAsync();
            }

            categoryId = category.Id;
        }

        var draft = new RecordDraft(
            type, status, occurredOn ?? new DateOnly(2026, 10, 15), accountId, amount, account.Currency, amount, categoryId,
            transferAccountId, transferAmount ?? (transferAccountId is null ? null : amount), name, tagIds ?? []);
        var context = await RecordContextLoader.LoadAsync(db, account.WalletId, draft, CancellationToken.None);
        var record = Record.Create(draft, context, createdBy ?? Guid.NewGuid(), DateTimeOffset.UtcNow).Value;
        db.Records.Add(record);
        await db.SaveChangesAsync();
        return record;
    }
}
