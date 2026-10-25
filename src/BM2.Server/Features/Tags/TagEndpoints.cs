using BM2.Domain.Common;
using BM2.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Tags;

public static class TagEndpoints
{
    public static RouteGroupBuilder MapTagEndpoints(this RouteGroupBuilder wallet)
    {
        var tags = wallet.MapGroup("/tags");
        CreateTag.Map(tags);
        UpdateTag.Map(tags);
        DeleteTag.Map(tags);
        return wallet;
    }
}

internal static class TagNames
{
    public static readonly Error TakenError = Error.Conflict("A tag with this name already exists.", "Name");

    public static Task<bool> IsTakenAsync(AppDbContext db, Guid walletId, string name, Guid? exceptId, CancellationToken ct)
    {
        var trimmed = name.Trim();
        return db.Tags.AnyAsync(t => t.WalletId == walletId && t.Name == trimmed && t.Id != exceptId, ct);
    }
}
