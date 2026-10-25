using BM2.Contracts.Wallets;

namespace BM2.Domain.Access;

/// <summary>What the current user may do in one wallet: a wallet role and/or roles on shared accounts.</summary>
public sealed class WalletAccess
{
    private readonly Dictionary<Guid, MemberRole> _accountRoles;

    private WalletAccess(Guid walletId, MemberRole? walletRole, Dictionary<Guid, MemberRole> accountRoles)
    {
        WalletId = walletId;
        WalletRole = walletRole;
        _accountRoles = accountRoles;
        ReadableAccountIds = accountRoles.Keys.ToArray();
    }

    public Guid WalletId { get; }

    public MemberRole? WalletRole { get; }

    public bool HasAnyAccess => WalletRole is not null || _accountRoles.Count > 0;

    public bool IsLimited => WalletRole is null;

    public IReadOnlyCollection<Guid> ReadableAccountIds { get; }

    /// <param name="accounts">Every account of the wallet with the user's share role on it, if any.</param>
    public static WalletAccess Create(
        Guid walletId, MemberRole? walletRole, IEnumerable<(Guid AccountId, MemberRole? ShareRole)> accounts)
    {
        var roles = new Dictionary<Guid, MemberRole>();
        foreach (var (accountId, shareRole) in accounts)
        {
            var effective = Max(walletRole, shareRole);
            if (effective is not null)
            {
                roles[accountId] = effective.Value;
            }
        }

        return new WalletAccess(walletId, walletRole, roles);
    }

    public bool Can(WalletPermission permission) => WalletRole switch
    {
        MemberRole.Owner => true,
        MemberRole.Editor => permission is WalletPermission.ManageCategories or WalletPermission.ViewMembers,
        MemberRole.Viewer => permission is WalletPermission.ViewMembers,
        _ => false,
    };

    public MemberRole? RoleFor(Guid accountId) => _accountRoles.TryGetValue(accountId, out var role) ? role : null;

    public bool CanReadAccount(Guid accountId) => RoleFor(accountId) is not null;

    public bool CanWriteAccount(Guid accountId) => RoleFor(accountId) >= MemberRole.Editor;

    public bool CanReadRecord(Guid accountId, Guid? transferAccountId) =>
        CanReadAccount(accountId) || (transferAccountId is { } target && CanReadAccount(target));

    public bool CanWriteRecord(Guid accountId, Guid? transferAccountId) =>
        CanWriteAccount(accountId) && (transferAccountId is not { } target || CanWriteAccount(target));

    private static MemberRole? Max(MemberRole? a, MemberRole? b) => a > b || b is null ? a : b;
}
