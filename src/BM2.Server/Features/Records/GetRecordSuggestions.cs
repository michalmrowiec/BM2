using BM2.Contracts.Records;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Records;

internal static class GetRecordSuggestions
{
    private const int NameCount = 200;
    private const int ScanCount = 1000;
    private const int UsageDays = 90;

    public static void Map(RouteGroupBuilder records) => records.MapGet("/suggestions", HandleAsync);

    private static async Task<Ok<RecordSuggestions>> HandleAsync(
        CurrentWallet current, AppDbContext db, TimeProvider time, CancellationToken ct)
    {
        var visible = db.Records.AsNoTracking().Visible(current.Access.ReadableAccountIds);

        // Distinct names are picked in memory from a bounded window of the latest named records.
        var named = await visible
            .Where(r => r.Name != null)
            .OrderByDescending(r => r.OccurredOn).ThenByDescending(r => r.CreatedAt)
            .Take(ScanCount)
            .Select(r => new { Name = r.Name!, r.Type, r.CategoryId, TagIds = r.Tags.Select(t => t.TagId).ToList() })
            .ToListAsync(ct);
        var names = named
            .DistinctBy(r => (r.Name.ToUpperInvariant(), r.Type))
            .Take(NameCount)
            .Select(r => new NameSuggestion(r.Name, r.Type, r.CategoryId, r.TagIds))
            .ToList();

        var since = DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime).AddDays(-UsageDays);
        var recent = visible.Where(r => r.OccurredOn >= since);
        var categoryIds = await recent
            .Where(r => r.CategoryId != null)
            .GroupBy(r => r.CategoryId!.Value)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .ToListAsync(ct);
        var tagIds = await recent
            .SelectMany(r => r.Tags)
            .GroupBy(t => t.TagId)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .ToListAsync(ct);

        return TypedResults.Ok(new RecordSuggestions(names, categoryIds, tagIds));
    }
}
