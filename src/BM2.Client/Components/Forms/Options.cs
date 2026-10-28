using BM2.Contracts.Common;
using BM2.Contracts.Wallets;
using static BM2.Client.Localization.Strings;

namespace BM2.Client.Components.Forms;

/// <summary>Option lists for SelectField.</summary>
public static class Options
{
    public static IReadOnlyList<(string Value, string Label)> Currencies { get; } =
        BM2.Contracts.Common.Currencies.All.Select(code => (code, code)).ToList();

    public static IReadOnlyList<(MemberRole Value, string Label)> WalletRoles =>
        [(MemberRole.Viewer, Tr("Viewer")), (MemberRole.Editor, Tr("Editor")), (MemberRole.Owner, Tr("Owner"))];

    public static IReadOnlyList<(MemberRole Value, string Label)> AccountRoles =>
        [(MemberRole.Viewer, Tr("Viewer")), (MemberRole.Editor, Tr("Editor"))];

    public static string RoleLabel(MemberRole? role) => role switch
    {
        MemberRole.Owner => Tr("Owner"),
        MemberRole.Editor => Tr("Editor"),
        MemberRole.Viewer => Tr("Viewer"),
        _ => Tr("Shared account"),
    };
}
