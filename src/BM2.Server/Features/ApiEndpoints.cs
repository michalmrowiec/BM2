using BM2.Server.Features.Accounts;
using BM2.Server.Features.Categories;
using BM2.Server.Features.Members;
using BM2.Server.Features.Profile;
using BM2.Server.Features.Records;
using BM2.Server.Features.Tags;
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
        wallet.MapCategoryEndpoints();
        wallet.MapTagEndpoints();
        wallet.MapRecordEndpoints();
        wallet.MapMemberEndpoints();

        // Unknown API routes must not fall through to the Blazor not-found page.
        app.MapFallback($"{ApiPaths.Prefix}/{{**path}}", () => TypedResults.NotFound());

        return api;
    }
}
