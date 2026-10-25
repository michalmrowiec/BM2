using BM2.Domain.Access;

namespace BM2.Server.Features.Wallets;

/// <summary>The wallet from the route with the current user's access, loaded by <see cref="WalletAccessFilter"/>.</summary>
public sealed class CurrentWallet
{
    private WalletAccess? _access;

    public Guid Id => Access.WalletId;

    public WalletAccess Access
    {
        get => _access ?? throw new InvalidOperationException("Wallet access is loaded only for endpoints in the wallet group.");
        set => _access = value;
    }
}
