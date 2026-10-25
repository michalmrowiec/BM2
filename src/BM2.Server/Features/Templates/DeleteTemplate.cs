using BM2.Domain.Common;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Templates;

internal static class DeleteTemplate
{
    public static void Map(RouteGroupBuilder templates) => templates.MapDelete("/{templateId:guid}", HandleAsync);

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        Guid templateId, CurrentWallet current, AppDbContext db, CancellationToken ct)
    {
        if (!await db.Templates.AnyAsync(t => t.Id == templateId && t.WalletId == current.Id, ct))
        {
            return Error.NotFound().ToProblem();
        }

        if (!TemplateRules.CanWrite(current.Access))
        {
            return Error.Forbidden().ToProblem();
        }

        await db.Templates.Where(t => t.Id == templateId).ExecuteDeleteAsync(ct);
        return TypedResults.NoContent();
    }
}
