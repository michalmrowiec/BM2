using BM2.Contracts.Profile;
using BM2.Server.Data;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Profile;

public static class GetMe
{
    public static RouteGroupBuilder MapProfileEndpoints(this RouteGroupBuilder api)
    {
        api.MapGet("/me", HandleAsync);
        return api;
    }

    private static async Task<Results<Ok<MeResponse>, UnauthorizedHttpResult>> HandleAsync(
        ICurrentUser currentUser, AppDbContext db, CancellationToken ct)
    {
        var userId = currentUser.UserId;
        var me = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new MeResponse(u.Id, u.Email!))
            .SingleOrDefaultAsync(ct);

        // A valid cookie can outlive its user (deleted account).
        return me is null ? TypedResults.Unauthorized() : TypedResults.Ok(me);
    }
}
