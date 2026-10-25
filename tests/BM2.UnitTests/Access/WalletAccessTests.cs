using BM2.Contracts.Wallets;
using BM2.Domain.Access;

namespace BM2.UnitTests.Access;

public class WalletAccessTests
{
    private static readonly Guid W = Guid.NewGuid(), A = Guid.NewGuid(), B = Guid.NewGuid();

    private static WalletAccess Member(MemberRole role) => WalletAccess.Create(W, role, [(A, null), (B, null)]);

    private static WalletAccess SharedA(MemberRole role) => WalletAccess.Create(W, null, [(A, role), (B, null)]);

    [Theory]
    [InlineData(MemberRole.Owner, true, true, true, true, true)]
    [InlineData(MemberRole.Editor, false, false, true, true, false)]
    [InlineData(MemberRole.Viewer, false, false, false, true, false)]
    public void Wallet_roles_follow_permission_matrix(
        MemberRole role, bool wallet, bool accounts, bool categories, bool viewMembers, bool manageMembers)
    {
        var access = Member(role);

        access.Can(WalletPermission.ManageWallet).ShouldBe(wallet);
        access.Can(WalletPermission.ManageAccounts).ShouldBe(accounts);
        access.Can(WalletPermission.ManageCategories).ShouldBe(categories);
        access.Can(WalletPermission.ViewMembers).ShouldBe(viewMembers);
        access.Can(WalletPermission.ManageMembers).ShouldBe(manageMembers);
    }

    [Fact]
    public void Viewer_reads_all_accounts_but_writes_none()
    {
        var access = Member(MemberRole.Viewer);

        access.ReadableAccountIds.ShouldBe([A, B], ignoreOrder: true);
        access.CanWriteAccount(A).ShouldBeFalse();
    }

    [Fact]
    public void Editor_writes_every_account()
    {
        var access = Member(MemberRole.Editor);

        access.CanWriteAccount(A).ShouldBeTrue();
        access.CanWriteAccount(B).ShouldBeTrue();
    }

    [Fact]
    public void Shared_account_editor_sees_and_writes_only_that_account()
    {
        var access = SharedA(MemberRole.Editor);

        access.ReadableAccountIds.ShouldBe([A]);
        access.CanWriteAccount(A).ShouldBeTrue();
        access.CanReadAccount(B).ShouldBeFalse();
    }

    [Fact]
    public void Shared_account_user_has_no_wallet_permissions()
    {
        var access = SharedA(MemberRole.Editor);

        access.IsLimited.ShouldBeTrue();
        Enum.GetValues<WalletPermission>().ShouldAllBe(p => !access.Can(p));
    }

    [Fact]
    public void Higher_of_wallet_role_and_share_role_wins() =>
        WalletAccess.Create(W, MemberRole.Viewer, [(A, MemberRole.Editor)]).CanWriteAccount(A).ShouldBeTrue();

    [Fact]
    public void Transfer_is_readable_with_either_side() => SharedA(MemberRole.Viewer).CanReadRecord(B, A).ShouldBeTrue();

    [Fact]
    public void Transfer_write_needs_both_sides() => SharedA(MemberRole.Editor).CanWriteRecord(A, B).ShouldBeFalse();

    [Fact]
    public void Record_on_writable_account_without_transfer_is_writable() =>
        SharedA(MemberRole.Editor).CanWriteRecord(A, null).ShouldBeTrue();

    [Fact]
    public void No_role_and_no_share_means_no_access() =>
        WalletAccess.Create(W, null, [(A, null)]).HasAnyAccess.ShouldBeFalse();

    [Fact]
    public void Unknown_account_is_not_readable() => Member(MemberRole.Owner).CanReadAccount(Guid.NewGuid()).ShouldBeFalse();
}
