using BM2.Contracts.Wallets;
using BM2.Domain.Accounts;

namespace BM2.UnitTests.Accounts;

public class AccountTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 24, 12, 0, 0, TimeSpan.Zero);

    private static Account NewAccount() => Account.Create(Guid.NewGuid(), "Cash", "PLN", 0, Now);

    [Fact]
    public void Currency_cannot_change_when_account_has_records() =>
        NewAccount().Update("Cash", "EUR", 0, false, hasRecords: true).Error!.Field.ShouldBe("Currency");

    [Fact]
    public void Currency_can_change_without_records()
    {
        var account = NewAccount();

        account.Update(" Bank ", "EUR", 10m, true, hasRecords: false).IsSuccess.ShouldBeTrue();

        account.ShouldSatisfyAllConditions(
            a => a.Name.ShouldBe("Bank"),
            a => a.Currency.ShouldBe("EUR"),
            a => a.OpeningBalance.ShouldBe(10m),
            a => a.IsArchived.ShouldBeTrue());
    }

    [Fact]
    public void Account_cannot_be_shared_as_owner() =>
        NewAccount().Grant(Guid.NewGuid(), MemberRole.Owner, Now).Error!.Field.ShouldBe("Role");

    [Fact]
    public void Share_grant_never_downgrades()
    {
        var account = NewAccount();
        var user = Guid.NewGuid();
        account.Grant(user, MemberRole.Editor, Now);

        account.Grant(user, MemberRole.Viewer, Now);

        account.Members.Single().Role.ShouldBe(MemberRole.Editor);
    }

    [Fact]
    public void Share_role_cannot_become_owner()
    {
        var account = NewAccount();
        var user = Guid.NewGuid();
        account.Grant(user, MemberRole.Viewer, Now);

        account.ChangeShareRole(user, MemberRole.Owner).Error!.Field.ShouldBe("Role");
    }

    [Fact]
    public void Removing_share_removes_member()
    {
        var account = NewAccount();
        var user = Guid.NewGuid();
        account.Grant(user, MemberRole.Viewer, Now);

        account.RemoveShare(user).IsSuccess.ShouldBeTrue();

        account.Members.ShouldBeEmpty();
    }
}
