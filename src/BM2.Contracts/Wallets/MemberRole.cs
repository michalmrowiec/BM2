namespace BM2.Contracts.Wallets;

/// <summary>Role in a wallet or on a shared account; the value orders roles by strength.</summary>
public enum MemberRole
{
    Viewer = 1,
    Editor = 2,
    Owner = 3,
}
