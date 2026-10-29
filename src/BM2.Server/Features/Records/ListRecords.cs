using BM2.Contracts.Records;
using BM2.Domain.Common;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Records;

internal static class ListRecords
{
    private const int MaxPageSize = 50;

    public static void Map(RouteGroupBuilder records) => records.MapGet("/", HandleAsync);

    private static async Task<Results<Ok<RecordPage>, ProblemHttpResult>> HandleAsync(
        [AsParameters] RecordFilter filter, CurrentWallet current, AppDbContext db, CancellationToken ct)
    {
        if (RecordQueries.Filtered(db, current, filter) is not { } query)
        {
            return Error.NotFound().ToProblem();
        }

        if (filter.Cursor is not null)
        {
            if (!RecordCursor.TryDecode(filter.Cursor, out var cursor))
            {
                return Error.Validation(nameof(RecordFilter.Cursor), "The cursor is not valid.").ToProblem();
            }

            query = query.Where(r => r.OccurredOn < cursor.OccurredOn
                || (r.OccurredOn == cursor.OccurredOn && (r.CreatedAt < cursor.CreatedAt
                    || (r.CreatedAt == cursor.CreatedAt && r.Id.CompareTo(cursor.Id) < 0))));
        }

        var pageSize = Math.Clamp(filter.Limit ?? MaxPageSize, 1, MaxPageSize);

        // Keys first, then the page's items: keeps the TOP query free of the tag collection.
        var keys = await Latest(query)
            .Take(pageSize + 1)
            .ToListAsync(ct);
        var page = keys.Take(pageSize).ToList();
        var next = keys.Count > pageSize ? page[^1].Encode() : null;
        return TypedResults.Ok(new RecordPage(await ItemsAsync(db, page, ct), next));
    }

    /// <summary>The newest <paramref name="count"/> records of the query.</summary>
    public static async Task<List<RecordItem>> LatestAsync(AppDbContext db, IQueryable<Domain.Records.Record> query, int count, CancellationToken ct) =>
        await ItemsAsync(db, await Latest(query).Take(count).ToListAsync(ct), ct);

    public static async Task<List<RecordItem>> EarliestAsync(AppDbContext db, IQueryable<Domain.Records.Record> query, int count, CancellationToken ct) =>
        await ItemsAsync(db, await query
            .OrderBy(r => r.OccurredOn).ThenBy(r => r.CreatedAt).ThenBy(r => r.Id)
            .Select(r => new RecordCursor(r.OccurredOn, r.CreatedAt, r.Id))
            .Take(count).ToListAsync(ct), ct);

    private static IQueryable<RecordCursor> Latest(IQueryable<Domain.Records.Record> query) => query
        .OrderByDescending(r => r.OccurredOn).ThenByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id)
        .Select(r => new RecordCursor(r.OccurredOn, r.CreatedAt, r.Id));

    private static async Task<List<RecordItem>> ItemsAsync(AppDbContext db, List<RecordCursor> keys, CancellationToken ct)
    {
        if (keys.Count == 0)
        {
            return [];
        }

        var ids = keys.Select(k => k.Id).ToList();
        var items = await db.Records
            .AsNoTracking()
            .Where(r => ids.Contains(r.Id))
            .Select(RecordQueries.ToItem)
            .ToDictionaryAsync(i => i.Id, ct);
        return keys.Select(k => items[k.Id]).ToList();
    }
}
