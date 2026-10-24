using BM2.Contracts.Accounts;
using BM2.Domain.Access;
using BM2.Domain.Accounts;
using BM2.Domain.Common;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Accounts;

internal static class CreateAccount
{
    public static void Map(RouteGroupBuilder accounts) => accounts.MapPost("/", HandleAsync).Validate<CreateAccountRequest>();

    private static async Task<Results<Created<AccountItem>, ProblemHttpResult>> HandleAsync(
        CreateAccountRequest request, CurrentWallet current, AppDbContext db, TimeProvider time, CancellationToken ct)
    {
        if (!current.Access.Can(WalletPermission.ManageAccounts))
        {
            return Error.Forbidden().ToProblem();
        }

        if (await AccountNames.IsTakenAsync(db, current.Id, request.Name, exceptId: null, ct))
        {
            return AccountNames.TakenError.ToProblem();
        }

        var account = Account.Create(current.Id, request.Name, request.Currency, request.OpeningBalance, time.GetUtcNow());
        db.Accounts.Add(account);
        await db.SaveChangesAsync(ct);

        return TypedResults.Created(
            $"{ApiPaths.Prefix}/wallets/{current.Id}/accounts/{account.Id}",
            new AccountItem(account.Id, account.Name, account.Currency, account.OpeningBalance, account.OpeningBalance, false, true));
    }
}

internal static class AccountNames
{
    public static readonly Error TakenError = Error.Conflict("An account with this name already exists.", "Name");

    public static Task<bool> IsTakenAsync(AppDbContext db, Guid walletId, string name, Guid? exceptId, CancellationToken ct)
    {
        var trimmed = name.Trim();
        return db.Accounts.AnyAsync(a => a.WalletId == walletId && a.Name == trimmed && a.Id != exceptId, ct);
    }
}
