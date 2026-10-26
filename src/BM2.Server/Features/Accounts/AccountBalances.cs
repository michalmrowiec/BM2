using BM2.Contracts.Records;
using BM2.Domain.Accounts;
using BM2.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Accounts;

internal static class AccountBalances
{
    /// <summary>
    /// Balance of each account in its own currency: opening balance plus completed and pending records
    /// (income +, expense and outgoing transfer −, incoming transfer + in the target currency).
    /// WithPlanned also adds planned records.
    /// </summary>
    public static async Task<Dictionary<Guid, (decimal Balance, decimal WithPlanned)>> GetAsync(
        AppDbContext db, IReadOnlyCollection<Guid> accountIds, CancellationToken ct)
    {
        var balances = await db.Accounts
            .AsNoTracking()
            .Where(a => accountIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => (a.OpeningBalance, a.OpeningBalance), ct);

        var own = await db.Records
            .Where(r => accountIds.Contains(r.AccountId))
            .GroupBy(r => new { r.AccountId, Planned = r.Status == RecordStatus.Planned })
            .Select(g => new { g.Key.AccountId, g.Key.Planned, Net = g.Sum(r => r.Type == RecordType.Income ? r.AccountAmount : -r.AccountAmount) })
            .ToListAsync(ct);

        var incoming = await db.Records
            .Where(r => r.TransferAccountId != null && accountIds.Contains(r.TransferAccountId.Value))
            .GroupBy(r => new { AccountId = r.TransferAccountId!.Value, Planned = r.Status == RecordStatus.Planned })
            .Select(g => new { g.Key.AccountId, g.Key.Planned, Net = g.Sum(r => r.TransferAmount ?? 0) })
            .ToListAsync(ct);

        foreach (var flow in own.Concat(incoming))
        {
            var (balance, withPlanned) = balances[flow.AccountId];
            balances[flow.AccountId] = (flow.Planned ? balance : balance + flow.Net, withPlanned + flow.Net);
        }

        return balances;
    }

    /// <summary>Latest valuation of each given account that has one.</summary>
    public static async Task<Dictionary<Guid, AccountValuation>> LatestValuationsAsync(
        AppDbContext db, IReadOnlyCollection<Guid> accountIds, CancellationToken ct)
    {
        if (accountIds.Count == 0)
        {
            return [];
        }

        var all = await db.AccountValuations.AsNoTracking()
            .Where(v => accountIds.Contains(v.AccountId) && !db.AccountValuations.Any(n => n.AccountId == v.AccountId && n.ValuedOn > v.ValuedOn))
            .ToListAsync(ct);
        return all.ToDictionary(v => v.AccountId);
    }
}
