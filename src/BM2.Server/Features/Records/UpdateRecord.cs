using BM2.Contracts.Records;
using BM2.Domain.Common;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Records;

internal static class UpdateRecord
{
    public static void Map(RouteGroupBuilder records) =>
        records.MapPut("/{recordId:guid}", HandleAsync).Validate<SaveRecordRequest>();

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        Guid recordId, SaveRecordRequest request, CurrentWallet current, AppDbContext db, TimeProvider time, CancellationToken ct)
    {
        var access = current.Access;
        var record = await db.Records.Include(r => r.Tags).SingleOrDefaultAsync(r => r.Id == recordId, ct);
        if (record is null || !access.CanReadRecord(record.AccountId, record.TransferAccountId))
        {
            return Error.NotFound().ToProblem();
        }

        // Both the record as it is and as it will be must be writable.
        if (!access.CanWriteRecord(record.AccountId, record.TransferAccountId))
        {
            return Error.Forbidden().ToProblem();
        }

        var draft = request.ToDraft();
        var context = await RecordContextLoader.LoadAsync(db, current.Id, draft, ct);
        if (RecordAccess.CheckWrite(access, draft, context) is { } denied)
        {
            return denied.ToProblem();
        }

        var result = record.Update(draft, context, time.GetUtcNow());
        if (!result.IsSuccess)
        {
            return result.Error!.ToProblem();
        }

        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}
