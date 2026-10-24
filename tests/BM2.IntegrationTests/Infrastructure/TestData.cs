using BM2.Contracts.Categories;
using BM2.Contracts.Wallets;
using BM2.Domain.Accounts;
using BM2.Domain.Categories;
using BM2.Domain.Tags;
using BM2.Domain.Wallets;
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
}
