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
    private const int PageSize = 50;

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

        // Keys first, then the page's items: keeps the TOP query free of the tag collection.
        var keys = await query
            .OrderByDescending(r => r.OccurredOn).ThenByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id)
            .Select(r => new RecordCursor(r.OccurredOn, r.CreatedAt, r.Id))
            .Take(PageSize + 1)
            .ToListAsync(ct);
        var page = keys.Take(PageSize).ToList();
        var ids = page.Select(k => k.Id).ToList();
        var items = await db.Records
            .AsNoTracking()
            .Where(r => ids.Contains(r.Id))
            .Select(RecordQueries.ToItem)
            .ToDictionaryAsync(i => i.Id, ct);

        var next = keys.Count > PageSize ? page[^1].Encode() : null;
        return TypedResults.Ok(new RecordPage(page.Select(k => items[k.Id]).ToList(), next));
    }
}
