using BM2.Contracts.Wallets;
using BM2.Domain.Access;
using BM2.Domain.Common;
using BM2.Server.Data;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Wallets;

internal static class UpdateWallet
{
    public static void Map(RouteGroupBuilder wallet) => wallet.MapPut("/", HandleAsync).Validate<UpdateWalletRequest>();

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        UpdateWalletRequest request, CurrentWallet current, AppDbContext db, CancellationToken ct)
    {
        if (!current.Access.Can(WalletPermission.ManageWallet))
        {
            return Error.Forbidden().ToProblem();
        }

        var wallet = await db.Wallets.SingleAsync(w => w.Id == current.Id, ct);
        wallet.Update(request.Name, request.BaseCurrency);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}
