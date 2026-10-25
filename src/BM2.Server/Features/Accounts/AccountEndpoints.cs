namespace BM2.Server.Features.Accounts;

public static class AccountEndpoints
{
    public static RouteGroupBuilder MapAccountEndpoints(this RouteGroupBuilder wallet)
    {
        var accounts = wallet.MapGroup("/accounts");
        CreateAccount.Map(accounts);
        UpdateAccount.Map(accounts);
        DeleteAccount.Map(accounts);
        return wallet;
    }
}
