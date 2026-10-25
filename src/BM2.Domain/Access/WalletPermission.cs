using System.Diagnostics.CodeAnalysis;

namespace BM2.Domain.Access;

[SuppressMessage("Naming", "CA1711", Justification = "Domain term, not a code access security permission.")]
public enum WalletPermission
{
    ManageWallet,
    ManageAccounts,
    ManageCategories,
    ViewMembers,
    ManageMembers,
}
