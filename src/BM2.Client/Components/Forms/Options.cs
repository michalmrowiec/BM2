using BM2.Contracts.Common;
using BM2.Contracts.Wallets;

namespace BM2.Client.Components.Forms;

/// <summary>Option lists for SelectField.</summary>
public static class Options
{
    public static IReadOnlyList<(string Value, string Label)> Currencies { get; } =
        BM2.Contracts.Common.Currencies.All.Select(code => (code, code)).ToList();

    public static IReadOnlyList<(MemberRole Value, string Label)> WalletRoles { get; } =
        [(MemberRole.Viewer, "Viewer"), (MemberRole.Editor, "Editor"), (MemberRole.Owner, "Owner")];

    public static IReadOnlyList<(MemberRole Value, string Label)> AccountRoles { get; } =
        [(MemberRole.Viewer, "Viewer"), (MemberRole.Editor, "Editor")];

    public static string RoleLabel(MemberRole? role) => role switch
    {
        MemberRole.Owner => "Owner",
        MemberRole.Editor => "Editor",
        MemberRole.Viewer => "Viewer",
        _ => "Shared account",
    };
}
