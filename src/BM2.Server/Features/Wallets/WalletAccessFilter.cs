using BM2.Contracts.Wallets;
using BM2.Domain.Access;
using BM2.Domain.Common;
using BM2.Server.Data;
using BM2.Server.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Wallets;

/// <summary>Loads the user's access to the wallet in the route; no access at all looks like a missing wallet (404).</summary>
public sealed class WalletAccessFilter(AppDbContext db, ICurrentUser currentUser, CurrentWallet currentWallet) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (!Guid.TryParse(context.HttpContext.GetRouteValue("walletId") as string, out var walletId))
        {
            return Error.NotFound().ToProblem();
        }

        var userId = currentUser.UserId;
        var wallet = await db.Wallets
            .AsNoTracking()
            .Where(w => w.Id == walletId)
            .Select(w => new
            {
                Role = w.Members.Where(m => m.UserId == userId).Select(m => (MemberRole?)m.Role).FirstOrDefault(),
                Accounts = db.Accounts
                    .Where(a => a.WalletId == w.Id)
                    .Select(a => new
                    {
                        a.Id,
                        ShareRole = a.Members.Where(m => m.UserId == userId).Select(m => (MemberRole?)m.Role).FirstOrDefault(),
                    })
                    .ToList(),
            })
            .SingleOrDefaultAsync(context.HttpContext.RequestAborted);

        var access = wallet is null
            ? null
            : WalletAccess.Create(walletId, wallet.Role, wallet.Accounts.Select(a => (a.Id, a.ShareRole)));
        if (access is not { HasAnyAccess: true })
        {
            return Error.NotFound().ToProblem();
        }

        currentWallet.Access = access;
        return await next(context);
    }
}
