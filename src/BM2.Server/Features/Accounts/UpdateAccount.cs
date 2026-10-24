using BM2.Contracts.Accounts;
using BM2.Domain.Access;
using BM2.Domain.Common;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Accounts;

internal static class UpdateAccount
{
    public static void Map(RouteGroupBuilder accounts) =>
        accounts.MapPut("/{accountId:guid}", HandleAsync).Validate<UpdateAccountRequest>();

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        Guid accountId, UpdateAccountRequest request, CurrentWallet current, AppDbContext db, CancellationToken ct)
    {
        if (!current.Access.Can(WalletPermission.ManageAccounts))
        {
            return Error.Forbidden().ToProblem();
        }

        var account = await db.Accounts.SingleOrDefaultAsync(a => a.Id == accountId && a.WalletId == current.Id, ct);
        if (account is null)
        {
            return Error.NotFound().ToProblem();
        }

        if (await AccountNames.IsTakenAsync(db, current.Id, request.Name, account.Id, ct))
        {
            return AccountNames.TakenError.ToProblem();
        }

        var hasRecords = await db.Records.AnyAsync(r => r.AccountId == account.Id || r.TransferAccountId == account.Id, ct);
        var result = account.Update(request.Name, request.Currency, request.OpeningBalance, request.IsArchived, hasRecords);
        if (!result.IsSuccess)
        {
            return result.Error!.ToProblem();
        }

        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}
