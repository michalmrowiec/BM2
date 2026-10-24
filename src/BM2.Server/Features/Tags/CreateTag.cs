using BM2.Contracts.Tags;
using BM2.Domain.Access;
using BM2.Domain.Common;
using BM2.Domain.Tags;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BM2.Server.Features.Tags;

internal static class CreateTag
{
    public static void Map(RouteGroupBuilder tags) => tags.MapPost("/", HandleAsync).Validate<CreateTagRequest>();

    private static async Task<Results<Created<TagItem>, ProblemHttpResult>> HandleAsync(
        CreateTagRequest request, CurrentWallet current, AppDbContext db, CancellationToken ct)
    {
        if (!current.Access.Can(WalletPermission.ManageCategories))
        {
            return Error.Forbidden().ToProblem();
        }

        if (await TagNames.IsTakenAsync(db, current.Id, request.Name, exceptId: null, ct))
        {
            return TagNames.TakenError.ToProblem();
        }

        var tag = Tag.Create(current.Id, request.Name, request.Color);
        db.Tags.Add(tag);
        await db.SaveChangesAsync(ct);

        return TypedResults.Created(
            $"{ApiPaths.Prefix}/wallets/{current.Id}/tags/{tag.Id}", new TagItem(tag.Id, tag.Name, false, tag.Color));
    }
}
