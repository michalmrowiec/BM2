using BM2.Client.Components;
using BM2.ClientTests.Infrastructure;

namespace BM2.ClientTests.Components;

public class WalletSwitcherTests : BunitContext
{
    private static readonly Guid HomeId = Guid.NewGuid(), FamilyId = Guid.NewGuid();
    private readonly StubApi _api;

    public WalletSwitcherTests() =>
        _api = this.UseBm2Client().WithWallets(Snapshots.Owner(HomeId, "Home"), Snapshots.Owner(FamilyId, "Family"));

    [Fact]
    public void Shows_current_wallet_name()
    {
        var cut = Render<WalletSwitcher>();

        cut.WaitForAssertion(() => cut.Find("button.wallet-switcher").TextContent.ShouldContain("Home"));
    }

    [Fact]
    public void Marks_current_wallet_with_aria_current()
    {
        var cut = Render<WalletSwitcher>();
        cut.WaitForElement("button.wallet-switcher").Click();

        cut.Find("[aria-current=true]").TextContent.ShouldContain("Home");
    }

    [Fact]
    public void Selecting_wallet_loads_its_snapshot_and_remembers_it()
    {
        var cut = Render<WalletSwitcher>();
        cut.WaitForElement("button.wallet-switcher").Click();

        cut.FindAll(".sheet button.list-row").Single(b => b.TextContent.Contains("Family")).Click();

        cut.WaitForAssertion(() => cut.Find("button.wallet-switcher").TextContent.ShouldContain("Family"));
        _api.Requests.ShouldContain($"GET api/wallets/{FamilyId}");
        JSInterop.VerifyInvoke("localStorage.setItem").Arguments.ShouldBe(["bm2-wallet", FamilyId.ToString()]);
    }
}
