using BM2.Contracts.Tags;
using BM2.Domain.Access;
using BM2.Domain.Common;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Tags;

internal static class UpdateTag
{
    public static void Map(RouteGroupBuilder tags) => tags.MapPut("/{tagId:guid}", HandleAsync).Validate<UpdateTagRequest>();

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        Guid tagId, UpdateTagRequest request, CurrentWallet current, AppDbContext db, CancellationToken ct)
    {
        if (!current.Access.Can(WalletPermission.ManageCategories))
        {
            return Error.Forbidden().ToProblem();
        }

        var tag = await db.Tags.SingleOrDefaultAsync(t => t.Id == tagId && t.WalletId == current.Id, ct);
        if (tag is null)
        {
            return Error.NotFound().ToProblem();
        }

        if (await TagNames.IsTakenAsync(db, current.Id, request.Name, tag.Id, ct))
        {
            return TagNames.TakenError.ToProblem();
        }

        tag.Update(request.Name, request.IsArchived);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}
