using BM2.Contracts.Categories;
using BM2.Domain.Access;
using BM2.Domain.Categories;
using BM2.Domain.Common;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BM2.Server.Features.Categories;

internal static class CreateCategory
{
    public static void Map(RouteGroupBuilder categories) =>
        categories.MapPost("/", HandleAsync).Validate<CreateCategoryRequest>();

    private static async Task<Results<Created<CategoryItem>, ProblemHttpResult>> HandleAsync(
        CreateCategoryRequest request, CurrentWallet current, AppDbContext db, CancellationToken ct)
    {
        if (!current.Access.Can(WalletPermission.ManageCategories))
        {
            return Error.Forbidden().ToProblem();
        }

        if (await CategoryNames.IsTakenAsync(db, current.Id, request.Type, request.Name, exceptId: null, ct))
        {
            return CategoryNames.TakenError.ToProblem();
        }

        var category = Category.Create(current.Id, request.Name, request.Type, request.Emoji);
        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);

        return TypedResults.Created(
            $"{ApiPaths.Prefix}/wallets/{current.Id}/categories/{category.Id}",
            new CategoryItem(category.Id, category.Name, category.Type, false, category.Emoji));
    }
}
