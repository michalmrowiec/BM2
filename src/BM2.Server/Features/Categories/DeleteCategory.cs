using BM2.Domain.Access;
using BM2.Domain.Common;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Categories;

internal static class DeleteCategory
{
    public static void Map(RouteGroupBuilder categories) => categories.MapDelete("/{categoryId:guid}", HandleAsync);

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        Guid categoryId, CurrentWallet current, AppDbContext db, CancellationToken ct)
    {
        if (!current.Access.Can(WalletPermission.ManageCategories))
        {
            return Error.Forbidden().ToProblem();
        }

        if (!await db.Categories.AnyAsync(c => c.Id == categoryId && c.WalletId == current.Id, ct))
        {
            return Error.NotFound().ToProblem();
        }

        if (await db.Records.AnyAsync(r => r.CategoryId == categoryId, ct))
        {
            return Error.Conflict("This category has records. Archive it instead.").ToProblem();
        }

        await db.Categories.Where(c => c.Id == categoryId).ExecuteDeleteAsync(ct);
        return TypedResults.NoContent();
    }
}
