using BM2.Contracts.Members;
using BM2.Domain.Access;
using BM2.Domain.Common;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Members;

internal static class UpdateMemberRole
{
    public static void Map(RouteGroupBuilder wallet) =>
        wallet.MapPut("/members/{userId:guid}", HandleAsync).Validate<UpdateRoleRequest>();

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        Guid userId, UpdateRoleRequest request, CurrentWallet current, AppDbContext db, CancellationToken ct)
    {
        if (!current.Access.Can(WalletPermission.ManageMembers))
        {
            return Error.Forbidden().ToProblem();
        }

        var wallet = await db.Wallets.Include(w => w.Members).SingleAsync(w => w.Id == current.Id, ct);
        var result = wallet.ChangeMemberRole(userId, request.Role);
        if (!result.IsSuccess)
        {
            return result.Error!.ToProblem();
        }

        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}
