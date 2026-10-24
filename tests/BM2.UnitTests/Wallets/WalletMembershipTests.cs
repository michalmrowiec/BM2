using BM2.Contracts.Wallets;
using BM2.Domain.Common;
using BM2.Domain.Wallets;

namespace BM2.UnitTests.Wallets;

public class WalletMembershipTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 24, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid Owner = Guid.NewGuid(), Other = Guid.NewGuid();

    private static Wallet NewWallet() => Wallet.Create("Home", "PLN", Owner, Now);

    [Fact]
    public void Creator_becomes_owner() => NewWallet().Members.Single().ShouldSatisfyAllConditions(
        m => m.UserId.ShouldBe(Owner),
        m => m.Role.ShouldBe(MemberRole.Owner));

    [Fact]
    public void Last_owner_cannot_leave() => NewWallet().RemoveMember(Owner).Error!.Kind.ShouldBe(ErrorKind.Conflict);

    [Fact]
    public void Last_owner_cannot_be_demoted() =>
        NewWallet().ChangeMemberRole(Owner, MemberRole.Editor).Error!.Kind.ShouldBe(ErrorKind.Conflict);

    [Fact]
    public void Owner_can_leave_when_another_owner_exists()
    {
        var wallet = NewWallet();
        wallet.Grant(Other, MemberRole.Owner, Now);

        wallet.RemoveMember(Owner).IsSuccess.ShouldBeTrue();
        wallet.Members.Single().UserId.ShouldBe(Other);
    }

    [Fact]
    public void Grant_never_downgrades()
    {
        var wallet = NewWallet();

        wallet.Grant(Owner, MemberRole.Viewer, Now);

        wallet.Members.Single().Role.ShouldBe(MemberRole.Owner);
    }

    [Fact]
    public void Grant_upgrades_existing_member()
    {
        var wallet = NewWallet();
        wallet.Grant(Other, MemberRole.Viewer, Now);

        wallet.Grant(Other, MemberRole.Editor, Now);

        wallet.Members.Single(m => m.UserId == Other).Role.ShouldBe(MemberRole.Editor);
    }

    [Fact]
    public void Owner_changes_member_role()
    {
        var wallet = NewWallet();
        wallet.Grant(Other, MemberRole.Editor, Now);

        wallet.ChangeMemberRole(Other, MemberRole.Viewer).IsSuccess.ShouldBeTrue();

        wallet.Members.Single(m => m.UserId == Other).Role.ShouldBe(MemberRole.Viewer);
    }

    [Fact]
    public void Removing_unknown_member_is_not_found() =>
        NewWallet().RemoveMember(Other).Error!.Kind.ShouldBe(ErrorKind.NotFound);

    [Fact]
    public void Names_are_trimmed() => Wallet.Create("  Home ", "PLN", Owner, Now).Name.ShouldBe("Home");
}
