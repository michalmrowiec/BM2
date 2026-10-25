using BM2.Domain.Common;
using BM2.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features;

/// <summary>Caps that keep one wallet (or one prank) from loading the server.</summary>
internal static class WalletLimits
{
    public const int Records = 200_000;
    public const int RecurringRules = 200;

    /// <summary>Occurrences one catch-up adds; the rest follow on the next wallet load.</summary>
    public const int CatchUpPerRun = 500;

    public const int RuleStartYearsBack = 2;

    public static Task<int> RecordCountAsync(AppDbContext db, Guid walletId, CancellationToken ct) =>
        db.Records.CountAsync(r => r.Account.WalletId == walletId, ct);

    public static Error TooManyRecords() =>
        Error.Conflict($"This wallet has reached the limit of {Records:N0} records. Delete old records to add new ones.");
}
