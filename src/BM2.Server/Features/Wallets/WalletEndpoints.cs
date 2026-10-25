namespace BM2.Server.Features.Wallets;

public static class WalletEndpoints
{
    /// <summary>Maps /wallets and returns the /wallets/{walletId} group that wallet features attach to.</summary>
    public static RouteGroupBuilder MapWalletEndpoints(this RouteGroupBuilder api)
    {
        var wallets = api.MapGroup("/wallets");
        ListWallets.Map(wallets);
        CreateWallet.Map(wallets);

        var wallet = wallets.MapGroup("/{walletId:guid}").AddEndpointFilter<WalletAccessFilter>();
        GetWallet.Map(wallet);
        UpdateWallet.Map(wallet);
        DeleteWallet.Map(wallet);
        return wallet;
    }
}
