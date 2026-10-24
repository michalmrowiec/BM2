using BM2.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Accounts;

internal static class AccountBalances
{
    /// <summary>Current balance of each account, in its own currency.</summary>
    public static async Task<Dictionary<Guid, decimal>> GetAsync(
        AppDbContext db, IReadOnlyCollection<Guid> accountIds, CancellationToken ct) =>
        await db.Accounts
            .AsNoTracking()
            .Where(a => accountIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => a.OpeningBalance, ct);
}
