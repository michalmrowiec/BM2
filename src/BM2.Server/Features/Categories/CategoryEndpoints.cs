using BM2.Contracts.Categories;
using BM2.Domain.Common;
using BM2.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Categories;

public static class CategoryEndpoints
{
    public static RouteGroupBuilder MapCategoryEndpoints(this RouteGroupBuilder wallet)
    {
        var categories = wallet.MapGroup("/categories");
        CreateCategory.Map(categories);
        UpdateCategory.Map(categories);
        DeleteCategory.Map(categories);
        return wallet;
    }
}

internal static class CategoryNames
{
    public static readonly Error TakenError = Error.Conflict("A category with this name already exists.", "Name");

    public static Task<bool> IsTakenAsync(
        AppDbContext db, Guid walletId, CategoryType type, string name, Guid? exceptId, CancellationToken ct)
    {
        var trimmed = name.Trim();
        return db.Categories.AnyAsync(
            c => c.WalletId == walletId && c.Type == type && c.Name == trimmed && c.Id != exceptId, ct);
    }
}
