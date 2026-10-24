using BM2.Contracts.Invitations;
using BM2.Contracts.Members;
using BM2.Domain.Access;
using BM2.Domain.Common;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Members;

internal static class ListMembers
{
    public static void Map(RouteGroupBuilder wallet) => wallet.MapGet("/members", HandleAsync);

    private static async Task<Results<Ok<MembersResponse>, ProblemHttpResult>> HandleAsync(
        CurrentWallet current, ICurrentUser currentUser, AppDbContext db, CancellationToken ct)
    {
        if (!current.Access.Can(WalletPermission.ViewMembers))
        {
            return Error.Forbidden().ToProblem();
        }

        var walletId = current.Id;
        var me = currentUser.UserId;
        var members = await db.WalletMembers
            .AsNoTracking()
            .Where(m => m.WalletId == walletId)
            .Join(db.Users, m => m.UserId, u => u.Id, (m, u) => new { m.Role, u.Id, u.Email })
            .OrderByDescending(m => m.Role).ThenBy(m => m.Email)
            .Select(m => new MemberItem(m.Id, m.Email!, m.Role, m.Id == me))
            .ToListAsync(ct);

        var shares = await db.AccountMembers
            .AsNoTracking()
            .Join(db.Accounts.Where(a => a.WalletId == walletId), m => m.AccountId, a => a.Id, (m, a) => new { m, a })
            .Join(db.Users, x => x.m.UserId, u => u.Id, (x, u) => new { x.a.Id, x.a.Name, UserId = u.Id, u.Email, x.m.Role })
            .OrderBy(s => s.Name).ThenBy(s => s.Email)
            .Select(s => new AccountShareItem(s.Id, s.Name, s.UserId, s.Email!, s.Role, s.UserId == me))
            .ToListAsync(ct);

        IReadOnlyList<InvitationItem> invitations = [];
        return TypedResults.Ok(new MembersResponse(members, shares, invitations));
    }
}
