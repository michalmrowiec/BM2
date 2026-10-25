using BM2.Contracts.Accounts;
using BM2.Contracts.Wallets;

namespace BM2.ClientTests.Infrastructure;

public static class Snapshots
{
    public static readonly WalletPermissions OwnerPermissions = new(true, true, true, true, true);

    public static WalletSnapshot Owner(Guid id, string name = "Home", IReadOnlyList<AccountItem>? accounts = null) =>
        new(id, name, "PLN", MemberRole.Owner, OwnerPermissions, accounts ?? [], [], []);

    /// <summary>Wallet list plus the snapshot of each wallet.</summary>
    public static StubApi WithWallets(this StubApi api, params WalletSnapshot[] snapshots)
    {
        api.OnGet("api/wallets", snapshots.Select(s => new WalletListItem(s.Id, s.Name, s.BaseCurrency, s.Role)).ToList());
        foreach (var snapshot in snapshots)
        {
            api.OnGet($"api/wallets/{snapshot.Id}", snapshot);
        }

        return api;
    }
}
