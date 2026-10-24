using BM2.Domain.Access;
using BM2.Domain.Common;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Tags;

internal static class DeleteTag
{
    public static void Map(RouteGroupBuilder tags) => tags.MapDelete("/{tagId:guid}", HandleAsync);

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        Guid tagId, CurrentWallet current, AppDbContext db, CancellationToken ct)
    {
        if (!current.Access.Can(WalletPermission.ManageCategories))
        {
            return Error.Forbidden().ToProblem();
        }

        var deleted = await db.Tags.Where(t => t.Id == tagId && t.WalletId == current.Id).ExecuteDeleteAsync(ct);
        return deleted == 0 ? Error.NotFound().ToProblem() : TypedResults.NoContent();
    }
}
