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
        Guid recordId, CurrentWallet current, ICurrentUser currentUser, AppDbContext db, CancellationToken ct, bool pair = false)
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

        // pair=true also deletes the other record of a "Send money" pair, when the user may write it.
        if (pair && await db.Records.SingleAsync(r => r.Id == recordId, ct) is { PairId: not null } paired
            && await RecordPairs.FindWritablePartnerAsync(db, currentUser.UserId, paired, ct) is { } partner)
        {
            db.Records.Remove(partner);
            await db.SaveChangesAsync(ct);
        }

        await db.Records.Where(r => r.Id == recordId).ExecuteDeleteAsync(ct);
        return TypedResults.NoContent();
    }
}
