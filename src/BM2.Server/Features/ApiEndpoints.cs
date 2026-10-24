using BM2.Server.Features.Accounts;
using BM2.Server.Features.Profile;
using BM2.Server.Features.Wallets;
using BM2.Server.Infrastructure;

namespace BM2.Server.Features;

public static class ApiEndpoints
{
    public static RouteGroupBuilder MapBm2Api(this WebApplication app)
    {
        var api = app.MapGroup(ApiPaths.Prefix).RequireAuthorization();
        api.MapProfileEndpoints();
        var wallet = api.MapWalletEndpoints();
        wallet.MapAccountEndpoints();

        // Unknown API routes must not fall through to the Blazor not-found page.
        app.MapFallback($"{ApiPaths.Prefix}/{{**path}}", () => TypedResults.NotFound());

        return api;
    }
}
