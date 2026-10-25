using BM2.Contracts.Wallets;
using BM2.Domain.Common;
using BM2.Domain.Invitations;
using BM2.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Invitations;

public static class InvitationAcceptance
{
    /// <summary>Usable, and its creator is still an owner: removing or demoting an owner voids their links.</summary>
    public static async Task<Result> CheckAsync(AppDbContext db, Invitation invitation, DateTimeOffset now, CancellationToken ct)
    {
        var usable = invitation.CheckUsable(now);
        if (!usable.IsSuccess)
        {
            return usable;
        }

        var creatorIsOwner = await db.WalletMembers.AnyAsync(
            m => m.WalletId == invitation.WalletId && m.UserId == invitation.CreatedByUserId && m.Role == MemberRole.Owner, ct);
        return creatorIsOwner ? Result.Success : Error.Gone("This invitation is no longer valid.");
    }

    /// <summary>
    /// Uses up the invitation and grants its role (an existing higher role stays). Changes are tracked, not saved.
    /// </summary>
    public static async Task<Result> AcceptAsync(
        AppDbContext db, Invitation invitation, Guid userId, DateTimeOffset now, CancellationToken ct)
    {
        var valid = await CheckAsync(db, invitation, now, ct);
        if (!valid.IsSuccess)
        {
            return valid;
        }

        var accepted = invitation.Accept(userId, now);
        if (!accepted.IsSuccess)
        {
            return accepted;
        }

        if (invitation.AccountId is { } accountId)
        {
            var account = await db.Accounts.Include(a => a.Members).SingleAsync(a => a.Id == accountId, ct);
            return account.Grant(userId, invitation.Role, now);
        }

        var wallet = await db.Wallets.Include(w => w.Members).SingleAsync(w => w.Id == invitation.WalletId, ct);
        wallet.Grant(userId, invitation.Role, now);
        return Result.Success;
    }
}
