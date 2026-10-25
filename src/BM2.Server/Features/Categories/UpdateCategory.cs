using BM2.Contracts.Categories;
using BM2.Domain.Access;
using BM2.Domain.Common;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Categories;

internal static class UpdateCategory
{
    public static void Map(RouteGroupBuilder categories) =>
        categories.MapPut("/{categoryId:guid}", HandleAsync).Validate<UpdateCategoryRequest>();

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        Guid categoryId, UpdateCategoryRequest request, CurrentWallet current, AppDbContext db, CancellationToken ct)
    {
        if (!current.Access.Can(WalletPermission.ManageCategories))
        {
            return Error.Forbidden().ToProblem();
        }

        var category = await db.Categories.SingleOrDefaultAsync(c => c.Id == categoryId && c.WalletId == current.Id, ct);
        if (category is null)
        {
            return Error.NotFound().ToProblem();
        }

        if (await CategoryNames.IsTakenAsync(db, current.Id, category.Type, request.Name, category.Id, ct))
        {
            return CategoryNames.TakenError.ToProblem();
        }

        category.Update(request.Name, request.IsArchived, request.Icon);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}
