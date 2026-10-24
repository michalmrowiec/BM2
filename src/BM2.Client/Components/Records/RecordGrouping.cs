using BM2.Contracts.Records;

namespace BM2.Client.Components.Records;

public sealed record RecordDay(DateOnly Date, IReadOnlyList<RecordItem> Items, IReadOnlyList<(string Currency, decimal Net)> Totals);

public static class RecordGrouping
{
    /// <summary>
    /// Days newest first, keeping the order of records within a day. A day's net is income minus expenses per account
    /// currency; transfers and records on accounts the user cannot see are left out.
    /// </summary>
    public static IReadOnlyList<RecordDay> ByDay(IEnumerable<RecordItem> items, Func<Guid, string?> accountCurrency) =>
        items
            .GroupBy(i => i.OccurredOn)
            .OrderByDescending(g => g.Key)
            .Select(day => new RecordDay(
                day.Key,
                day.ToList(),
                day.Where(i => i.Type != RecordType.Transfer)
                    .Select(i => (Currency: accountCurrency(i.AccountId), Net: i.Type == RecordType.Income ? i.AccountAmount : -i.AccountAmount))
                    .Where(x => x.Currency is not null)
                    .GroupBy(x => x.Currency!)
                    .OrderBy(g => g.Key, StringComparer.Ordinal)
                    .Select(g => (g.Key, g.Sum(x => x.Net)))
                    .ToList()))
            .ToList();
}
