using BM2.Contracts.Categories;
using BM2.Contracts.Records;
using BM2.Domain.Common;
using BM2.Domain.Records;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Records;

/// <summary>
/// "Send money": an expense in one wallet and an income in another, linked by <see cref="Record.PairId"/>. The income
/// is added when the expense is completed, checked against the user's access to the other wallet at that moment.
/// </summary>
internal static class RecordPairs
{
    public static async Task<Error?> CheckAsync(AppDbContext db, Guid userId, Guid walletId, RecordPair pair, CancellationToken ct)
    {
        if (pair.WalletId == walletId)
        {
            return Error.Validation("Pair", "Choose another wallet.");
        }

        var access = await WalletAccessLoader.LoadAsync(db, userId, pair.WalletId, ct);
        if (access is not { HasAnyAccess: true } || !access.CanReadAccount(pair.AccountId))
        {
            return Error.NotFound();
        }

        if (!access.CanWriteAccount(pair.AccountId))
        {
            return Error.Forbidden();
        }

        var valid = await db.Accounts.AnyAsync(a => a.Id == pair.AccountId && a.WalletId == pair.WalletId && !a.IsArchived, ct)
            && await db.Categories.AnyAsync(
                c => c.Id == pair.CategoryId && c.WalletId == pair.WalletId && c.Type == CategoryType.Income && !c.IsArchived, ct);
        return valid ? null : Error.Validation("Pair", "Choose an active account and income category in the other wallet.");
    }

    /// <summary>Adds the income in the other wallet for a completed record with a pending pair.</summary>
    /// <returns>False when the other side can't be added now (no access any more); the record keeps the pending pair.</returns>
    public static async Task<bool> SendAsync(
        AppDbContext db, Guid userId, Guid walletId, Record record, DateTimeOffset now, CancellationToken ct)
    {
        if (record.Status != RecordStatus.Completed || !record.HasPendingPair)
        {
            return true;
        }

        var pair = new RecordPair(record.PairWalletId!.Value, record.PairAccountId!.Value, record.PairCategoryId!.Value, record.PairAmount!.Value);
        if (await CheckAsync(db, userId, walletId, pair, ct) is not null)
        {
            return false;
        }

        var currency = await db.Accounts.Where(a => a.Id == pair.AccountId).Select(a => a.Currency).SingleAsync(ct);
        var draft = new RecordDraft(
            RecordType.Income, RecordStatus.Completed, record.OccurredOn, pair.AccountId, pair.Amount, currency, pair.Amount,
            pair.CategoryId, null, null, record.Name, []);
        var context = await RecordContextLoader.LoadAsync(db, pair.WalletId, draft, ct);
        var created = Record.Create(draft, context, userId, now);
        if (!created.IsSuccess)
        {
            return false;
        }

        var pairId = Guid.NewGuid();
        created.Value.LinkPair(pairId, walletId);
        record.LinkPair(pairId, pair.WalletId);
        db.Records.Add(created.Value);
        return true;
    }

    /// <summary>The other record of the pair, when the user may write it.</summary>
    public static async Task<Record?> FindWritablePartnerAsync(AppDbContext db, Guid userId, Record record, CancellationToken ct)
    {
        if (record.PairId is not { } pairId || record.PairWalletId is not { } otherWalletId)
        {
            return null;
        }

        var partner = await db.Records.Include(r => r.Tags).SingleOrDefaultAsync(r => r.PairId == pairId && r.Id != record.Id, ct);
        if (partner is null)
        {
            return null;
        }

        var access = await WalletAccessLoader.LoadAsync(db, userId, otherWalletId, ct);
        return access is not null && access.CanWriteRecord(partner.AccountId, partner.TransferAccountId) ? partner : null;
    }

    /// <summary>Copies the date and name, and the amount when both accounts use one currency, to the other record.</summary>
    public static async Task UpdatePartnerAsync(AppDbContext db, Guid userId, Record record, DateTimeOffset now, CancellationToken ct)
    {
        if (await FindWritablePartnerAsync(db, userId, record, ct) is not { } partner)
        {
            return;
        }

        var currencies = await db.Accounts
            .Where(a => a.Id == record.AccountId || a.Id == partner.AccountId)
            .Select(a => a.Currency)
            .Distinct()
            .ToListAsync(ct);
        var draft = partner.ToDraft() with { OccurredOn = record.OccurredOn, Name = record.Name };
        if (currencies.Count == 1)
        {
            draft = draft with { Amount = record.AccountAmount, Currency = currencies[0], AccountAmount = record.AccountAmount };
        }

        var context = await RecordContextLoader.LoadAsync(db, record.PairWalletId!.Value, draft, ct);
        partner.Update(draft, context, now);
    }
}
