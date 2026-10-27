using System.Globalization;
using BM2.Client.State;
using BM2.Contracts.Records;
using BM2.Contracts.Stats;

namespace BM2.Client.Components.Stats;

/// <summary>
/// Pinned chart queries: the screen's filters as a query string. Dates are left out unless the period is custom, so a
/// pinned "this month" or "last 12 months" keeps moving; they are added when Home shows the chart.
/// </summary>
public static class PinnedChartQueries
{
    public static string Categories(StatsState state)
    {
        var custom = state.Period == StatsPeriod.Custom;
        var (from, to) = state.Range;
        return Build(
            ("period", state.Period.ToString()),
            ("from", custom ? Date(from) : null),
            ("to", custom ? Date(to) : null),
            ("accountId", state.AccountId?.ToString()),
            ("currency", state.Currency),
            ("type", state.Type.ToString()));
    }

    public static string Monthly(StatsState state, Guid? categoryId = null, IEnumerable<Guid>? tagIds = null) => Build(
        [("accountId", state.AccountId?.ToString()), ("currency", state.Currency), ("categoryId", categoryId?.ToString()),
            .. (tagIds ?? []).Select(id => ("tagIds", (string?)id.ToString()))]);

    public static string Forecast(StatsState state) =>
        new ForecastQuery { Currency = state.Currency, Months = state.ForecastMonths, Step = state.ForecastStep, Trend = state.ForecastTrend }
            .ToQueryString();

    /// <summary>The stats API call for a pin; Type is the record type a category chart shows.</summary>
    public static (string Url, RecordType Type) ApiUrl(Guid walletId, PinnedChartItem pin, DateOnly today)
    {
        var parts = Parse(pin.Query);
        var month = new DateOnly(today.Year, today.Month, 1);
        var type = Enum.TryParse<RecordType>(Value(parts, "type"), out var t) ? t : RecordType.Expense;
        var keep = parts.Where(p => p.Key is not ("period" or "type" or "today")).ToList();
        switch (pin.Kind)
        {
            case PinnedChartKind.Categories:
                var period = Value(parts, "period");
                if (period != nameof(StatsPeriod.Custom))
                {
                    var from = period == nameof(StatsPeriod.Year) ? new DateOnly(today.Year, 1, 1) : month;
                    keep.RemoveAll(p => p.Key is "from" or "to");
                    keep.Add(("from", Date(from)));
                    keep.Add(("to", Date(period == nameof(StatsPeriod.Year) ? from.AddYears(1) : from.AddMonths(1))));
                }

                return (Url(walletId, "categories", keep), type);
            case PinnedChartKind.Forecast:
                keep.Add(("today", Date(today)));
                return (Url(walletId, "forecast", keep), type);
            default:
                var (start, end) = StatsState.MonthsEndingWith(month);
                keep.RemoveAll(p => p.Key is "from" or "to");
                keep.Add(("from", Date(start)));
                keep.Add(("to", Date(end)));
                return (Url(walletId, "monthly", keep), type);
        }
    }

    private static string Url(Guid walletId, string endpoint, List<(string Key, string Value)> parts) =>
        $"api/wallets/{walletId}/stats/{endpoint}" + Build(parts.Select(p => (p.Key, (string?)p.Value)).ToArray());

    private static string Date(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static string? Value(List<(string Key, string Value)> parts, string key) =>
        parts.FirstOrDefault(p => p.Key == key).Value;

    private static string Build(params (string Key, string? Value)[] parts)
    {
        var present = parts.Where(p => !string.IsNullOrEmpty(p.Value)).Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value!)}").ToList();
        return present.Count == 0 ? "" : "?" + string.Join("&", present);
    }

    private static List<(string Key, string Value)> Parse(string query) => query.TrimStart('?')
        .Split('&', StringSplitOptions.RemoveEmptyEntries)
        .Select(p => p.Split('=', 2))
        .Select(p => (Uri.UnescapeDataString(p[0]), p.Length > 1 ? Uri.UnescapeDataString(p[1]) : ""))
        .ToList();
}
