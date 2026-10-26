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
        Guid recordId, SaveRecordRequest request, CurrentWallet current, ICurrentUser currentUser, AppDbContext db, TimeProvider time,
        CancellationToken ct)
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

        // Completing a "Send money" expense adds the income in the other wallet; an edit may carry over to it.
        var now = time.GetUtcNow();
        await RecordPairs.SendAsync(db, currentUser.UserId, current.Id, record, now, ct);
        if (request.UpdatePair)
        {
            await RecordPairs.UpdatePartnerAsync(db, currentUser.UserId, record, now, ct);
        }

        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}
