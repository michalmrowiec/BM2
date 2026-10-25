using BM2.Contracts.Wallets;
using BM2.Domain.Wallets;
using BM2.Server.Data;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BM2.Server.Features.Wallets;

internal static class CreateWallet
{
    public static void Map(RouteGroupBuilder wallets) => wallets.MapPost("/", HandleAsync).Validate<CreateWalletRequest>();

    private static async Task<Created<WalletListItem>> HandleAsync(
        CreateWalletRequest request, ICurrentUser currentUser, AppDbContext db, TimeProvider time, CancellationToken ct)
    {
        var wallet = Wallet.Create(request.Name, request.BaseCurrency, currentUser.UserId, time.GetUtcNow());
        db.Wallets.Add(wallet);
        db.Categories.AddRange(DefaultCategories.For(wallet.Id));
        await db.SaveChangesAsync(ct);

        return TypedResults.Created(
            $"{ApiPaths.Prefix}/wallets/{wallet.Id}",
            new WalletListItem(wallet.Id, wallet.Name, wallet.BaseCurrency, MemberRole.Owner));
    }
}
