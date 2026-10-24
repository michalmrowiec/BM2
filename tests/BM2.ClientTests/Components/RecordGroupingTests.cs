using BM2.Client.Components.Records;
using BM2.Contracts.Records;

namespace BM2.ClientTests.Components;

public class RecordGroupingTests
{
    private static readonly Guid Pln = Guid.NewGuid(), Eur = Guid.NewGuid();
    private static readonly DateOnly Oct30 = new(2026, 10, 30), Oct31 = new(2026, 10, 31);

    private static string? CurrencyOf(Guid accountId) => accountId == Pln ? "PLN" : accountId == Eur ? "EUR" : null;

    private static RecordItem Item(DateOnly day, RecordType type, Guid account, decimal amount, Guid? target = null) => new(
        Guid.NewGuid(), type, RecordStatus.Completed, day, account, "A", amount, "PLN", amount, null, target, null,
        target is null ? null : amount, null, []);

    [Fact]
    public void Groups_by_day_descending_with_net_per_account_currency()
    {
        var days = RecordGrouping.ByDay(
            [Item(Oct31, RecordType.Expense, Pln, 12.5m), Item(Oct30, RecordType.Income, Pln, 5000m), Item(Oct30, RecordType.Expense, Pln, 12.5m)],
            CurrencyOf);

        days.Select(d => d.Date).ShouldBe([Oct31, Oct30]);
        days[0].Totals.ShouldBe([("PLN", -12.5m)]);
        days[1].Totals.ShouldBe([("PLN", 4987.5m)]);
        days[1].Items.Count.ShouldBe(2);
    }

    [Fact]
    public void Transfers_do_not_change_day_total() =>
        RecordGrouping.ByDay([Item(Oct31, RecordType.Transfer, Pln, 100m, Eur)], CurrencyOf).Single().Totals.ShouldBeEmpty();

    [Fact]
    public void Separate_totals_for_each_currency() =>
        RecordGrouping.ByDay([Item(Oct31, RecordType.Expense, Eur, 20m), Item(Oct31, RecordType.Expense, Pln, 10m)], CurrencyOf)
            .Single().Totals.ShouldBe([("EUR", -20m), ("PLN", -10m)]);

    [Fact]
    public void Records_on_unknown_accounts_are_left_out_of_totals() =>
        RecordGrouping.ByDay([Item(Oct31, RecordType.Expense, Guid.NewGuid(), 20m)], CurrencyOf).Single().Totals.ShouldBeEmpty();
}
