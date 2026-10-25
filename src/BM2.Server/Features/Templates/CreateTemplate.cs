using BM2.Contracts.Templates;
using BM2.Domain.Templates;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BM2.Server.Features.Templates;

internal static class CreateTemplate
{
    public static void Map(RouteGroupBuilder templates) =>
        templates.MapPost("/", HandleAsync).Validate<SaveTemplateRequest>();

    private static async Task<Results<Created<TemplateItem>, ProblemHttpResult>> HandleAsync(
        SaveTemplateRequest request, CurrentWallet current, AppDbContext db, TimeProvider time, CancellationToken ct)
    {
        if (await TemplateRules.CheckAsync(db, current.Access, request, ct) is { } error)
        {
            return error.ToProblem();
        }

        var template = Template.Create(current.Id, request, time.GetUtcNow());
        db.Templates.Add(template);
        await db.SaveChangesAsync(ct);

        return TypedResults.Created(
            $"{ApiPaths.Prefix}/wallets/{current.Id}/templates/{template.Id}",
            new TemplateItem(
                template.Id, template.Type, template.AccountId, template.CategoryId, template.TransferAccountId, template.Amount,
                template.Currency, template.Name, template.Tags.Select(t => t.TagId).ToList(), template.IsPinned));
    }
}
