using BM2.Contracts.Templates;
using BM2.Domain.Common;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Templates;

internal static class UpdateTemplate
{
    public static void Map(RouteGroupBuilder templates) =>
        templates.MapPut("/{templateId:guid}", HandleAsync).Validate<SaveTemplateRequest>();

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        Guid templateId, SaveTemplateRequest request, CurrentWallet current, AppDbContext db, CancellationToken ct)
    {
        var template = await db.Templates
            .Include(t => t.Tags)
            .SingleOrDefaultAsync(t => t.Id == templateId && t.WalletId == current.Id, ct);
        if (template is null)
        {
            return Error.NotFound().ToProblem();
        }

        if (await TemplateRules.CheckAsync(db, current.Access, request, ct) is { } error)
        {
            return error.ToProblem();
        }

        template.Update(request);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}
