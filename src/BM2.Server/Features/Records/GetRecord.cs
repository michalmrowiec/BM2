using BM2.Contracts.Records;
using BM2.Domain.Common;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Records;

internal static class GetRecord
{
    public static void Map(RouteGroupBuilder records) => records.MapGet("/{recordId:guid}", HandleAsync);

    private static async Task<Results<Ok<RecordItem>, ProblemHttpResult>> HandleAsync(
        Guid recordId, CurrentWallet current, AppDbContext db, CancellationToken ct)
    {
        var item = await FindAsync(db, current, recordId, ct);
        return item is null ? Error.NotFound().ToProblem() : TypedResults.Ok(item);
    }

    public static Task<RecordItem?> FindAsync(AppDbContext db, CurrentWallet current, Guid recordId, CancellationToken ct) =>
        db.Records
            .AsNoTracking()
            .Where(r => r.Id == recordId)
            .Visible(current.Access.ReadableAccountIds)
            .Select(RecordQueries.ToItem)
            .SingleOrDefaultAsync(ct);
}
