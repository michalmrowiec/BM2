using BM2.Contracts.Accounts;

namespace BM2.Contracts.Wallets;

/// <param name="Role">Null when the user only has shared accounts in this wallet.</param>
public sealed record WalletListItem(Guid Id, string Name, string BaseCurrency, MemberRole? Role);

public sealed record WalletPermissions(
    bool ManageWallet, bool ManageAccounts, bool ManageCategories, bool ViewMembers, bool ManageMembers);

/// <summary>Everything a screen needs about one wallet, in a single call.</summary>
public sealed record WalletSnapshot(
    Guid Id,
    string Name,
    string BaseCurrency,
    MemberRole? Role,
    WalletPermissions Permissions,
    IReadOnlyList<AccountItem> Accounts);
