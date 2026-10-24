using BM2.Contracts.Records;
using BM2.Domain.Records;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BM2.Server.Features.Records;

internal static class CreateRecord
{
    public static void Map(RouteGroupBuilder records) => records.MapPost("/", HandleAsync).Validate<SaveRecordRequest>();

    private static async Task<Results<Created<RecordItem>, ProblemHttpResult>> HandleAsync(
        SaveRecordRequest request, CurrentWallet current, ICurrentUser currentUser, AppDbContext db, TimeProvider time,
        CancellationToken ct)
    {
        var draft = request.ToDraft();
        var context = await RecordContextLoader.LoadAsync(db, current.Id, draft, ct);
        if (RecordAccess.CheckWrite(current.Access, draft, context) is { } denied)
        {
            return denied.ToProblem();
        }

        var created = Record.Create(draft, context, currentUser.UserId, time.GetUtcNow());
        if (!created.IsSuccess)
        {
            return created.Error!.ToProblem();
        }

        db.Records.Add(created.Value);
        await db.SaveChangesAsync(ct);

        var item = await GetRecord.FindAsync(db, current, created.Value.Id, ct);
        return TypedResults.Created($"{ApiPaths.Prefix}/wallets/{current.Id}/records/{item!.Id}", item);
    }
}
