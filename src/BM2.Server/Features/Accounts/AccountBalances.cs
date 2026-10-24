using BM2.Contracts.Records;
using BM2.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Accounts;

internal static class AccountBalances
{
    /// <summary>
    /// Current balance of each account in its own currency: opening balance plus completed records
    /// (income +, expense and outgoing transfer −, incoming transfer + in the target currency).
    /// </summary>
    public static async Task<Dictionary<Guid, decimal>> GetAsync(
        AppDbContext db, IReadOnlyCollection<Guid> accountIds, CancellationToken ct)
    {
        var balances = await db.Accounts
            .AsNoTracking()
            .Where(a => accountIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => a.OpeningBalance, ct);

        var own = await db.Records
            .Where(r => r.Status == RecordStatus.Completed && accountIds.Contains(r.AccountId))
            .GroupBy(r => r.AccountId)
            .Select(g => new { AccountId = g.Key, Net = g.Sum(r => r.Type == RecordType.Income ? r.AccountAmount : -r.AccountAmount) })
            .ToListAsync(ct);

        var incoming = await db.Records
            .Where(r => r.Status == RecordStatus.Completed && r.TransferAccountId != null && accountIds.Contains(r.TransferAccountId.Value))
            .GroupBy(r => r.TransferAccountId!.Value)
            .Select(g => new { AccountId = g.Key, Net = g.Sum(r => r.TransferAmount ?? 0) })
            .ToListAsync(ct);

        foreach (var flow in own.Concat(incoming))
        {
            balances[flow.AccountId] += flow.Net;
        }

        return balances;
    }
}
