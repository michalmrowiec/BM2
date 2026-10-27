using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Records;

/// <summary>Months (yyyy-MM, newest first) that have visible records, for the month picker.</summary>
internal static class ListRecordMonths
{
    public static void Map(RouteGroupBuilder records) => records.MapGet("/months", HandleAsync);

    private static async Task<Ok<List<string>>> HandleAsync(CurrentWallet current, AppDbContext db, CancellationToken ct)
    {
        var months = await db.Records.AsNoTracking().Visible(current.Access.ReadableAccountIds)
            .GroupBy(r => new { r.OccurredOn.Year, r.OccurredOn.Month })
            .Select(g => g.Key)
            .ToListAsync(ct);

        return TypedResults.Ok(months
            .OrderByDescending(m => m.Year).ThenByDescending(m => m.Month)
            .Select(m => $"{m.Year:D4}-{m.Month:D2}")
            .ToList());
    }
}
