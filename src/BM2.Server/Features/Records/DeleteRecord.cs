using BM2.Domain.Common;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Records;

internal static class DeleteRecord
{
    public static void Map(RouteGroupBuilder records) => records.MapDelete("/{recordId:guid}", HandleAsync);

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        Guid recordId, CurrentWallet current, AppDbContext db, CancellationToken ct)
    {
        var access = current.Access;
        var record = await db.Records
            .AsNoTracking()
            .Where(r => r.Id == recordId)
            .Select(r => new { r.AccountId, r.TransferAccountId })
            .SingleOrDefaultAsync(ct);
        if (record is null || !access.CanReadRecord(record.AccountId, record.TransferAccountId))
        {
            return Error.NotFound().ToProblem();
        }

        if (!access.CanWriteRecord(record.AccountId, record.TransferAccountId))
        {
            return Error.Forbidden().ToProblem();
        }

        await db.Records.Where(r => r.Id == recordId).ExecuteDeleteAsync(ct);
        return TypedResults.NoContent();
    }
}
