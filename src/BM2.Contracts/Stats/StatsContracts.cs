using System.Globalization;
using BM2.Contracts.Records;

namespace BM2.Contracts.Stats;

/// <summary>
/// Stats query over completed income and expense records (transfers are skipped) on accounts in one currency.
/// <see cref="To"/> is exclusive; no currency means the wallet's base currency.
/// </summary>
public sealed class StatsFilter
{
    public DateOnly? From { get; init; }

    public DateOnly? To { get; init; }

    public Guid? AccountId { get; init; }

    public string? Currency { get; init; }

    public Guid? CategoryId { get; init; }

    /// <summary>Records with any of these tags.</summary>
    public Guid[]? TagIds { get; init; }

    public string ToQueryString()
    {
        var parts = new List<string>();
        Add(parts, "from", From?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        Add(parts, "to", To?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        Add(parts, "accountId", AccountId?.ToString());
        Add(parts, "currency", Currency);
        Add(parts, "categoryId", CategoryId?.ToString());
        foreach (var tagId in TagIds ?? [])
        {
            Add(parts, "tagIds", tagId.ToString());
        }

        return parts.Count == 0 ? "" : "?" + string.Join("&", parts);
    }

    private static void Add(List<string> parts, string name, string? value)
    {
        if (value is not null)
        {
            parts.Add($"{name}={Uri.EscapeDataString(value)}");
        }
    }
}

/// <param name="CategoryId">Null for records without a category.</param>
public sealed record CategoryTotal(Guid? CategoryId, RecordType Type, decimal Total, int Count);

public sealed record CategoryTotals(string Currency, IReadOnlyList<CategoryTotal> Items);

/// <param name="Month">First day of the month.</param>
/// <summary>Invested: transfers into investment accounts minus transfers out of them.</summary>
public sealed record MonthTotal(DateOnly Month, decimal Income, decimal Expense, decimal Invested = 0);

/// <summary>Every month of the range, oldest first, including empty ones.</summary>
public sealed record MonthlyTotals(string Currency, IReadOnlyList<MonthTotal> Items);

/// <param name="Month">First day of the month.</param>
public sealed record CategoryMonthTotal(Guid? CategoryId, RecordType Type, DateOnly Month, decimal Total);

/// <summary>Totals per category and month; months without records are left out.</summary>
public sealed record CategoryMonthTotals(string Currency, IReadOnlyList<CategoryMonthTotal> Items);

public enum ForecastStep
{
    Day = 1,
    Week = 2,
}

/// <summary>Balance forecast for accounts in one currency (no currency means the wallet's base currency).</summary>
/// <param name="Months">3, 6, 12 or 24.</param>
/// <param name="Trend">Add the income and spending trend from history (categories without recurring rules).</param>
/// <param name="Today">The device's date.</param>
public sealed class ForecastQuery
{
    public string? Currency { get; init; }

    public int? Months { get; init; }

    public ForecastStep? Step { get; init; }

    public bool? Trend { get; init; }

    public DateOnly? Today { get; init; }

    public string ToQueryString() => "?" + string.Join("&", new[]
    {
        Currency is null ? null : $"currency={Uri.EscapeDataString(Currency)}",
        Months is null ? null : $"months={Months.Value.ToString(CultureInfo.InvariantCulture)}",
        Step is null ? null : $"step={Step}",
        Trend is null ? null : $"trend={(Trend.Value ? "true" : "false")}",
        Today is null ? null : $"today={Today.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}",
    }.Where(p => p is not null));
}

/// <param name="Spread">Half-width of the ~80% uncertainty band around <paramref name="Balance"/>.</param>
public sealed record ForecastPoint(DateOnly Date, decimal Balance, decimal Spread = 0);

/// <summary>How much history the trend could use: none, an average (under 6 months), a line, a line with seasons (12+).</summary>
public enum ForecastModel
{
    None = 0,
    Average = 1,
    Linear = 2,
    Seasonal = 3,
}

/// <summary>Trend of income and spending (refunds netted) per month, without recurring rules and their categories.</summary>
/// <param name="From">First month of history used.</param>
/// <param name="Income">Expected income this month; <paramref name="IncomeSlope"/> is the change per month.</param>
/// <param name="MonthlySpread">Half-width of the ~80% band for one month of net flow.</param>
/// <param name="ExcludedCategoryCount">Categories left to recurring rules.</param>
public sealed record ForecastTrend(
    ForecastModel Model,
    DateOnly From,
    int Months,
    decimal Income,
    decimal IncomeSlope,
    decimal Expense,
    decimal ExpenseSlope,
    decimal MonthlySpread,
    int ExcludedCategoryCount);

/// <summary>One calendar month of the forecast (the first and last are partial): trend plus known items.</summary>
/// <param name="Balance">Balance at the month's last forecast day.</param>
public sealed record ForecastMonth(DateOnly Month, decimal Income, decimal Expense, decimal Balance, decimal Spread);

public enum ForecastSource
{
    Recurring = 1,
    Planned = 2,
    Pending = 3,
    Completed = 4,
}

/// <summary>One recurring rule (all its occurrences) or one dated record; <paramref name="Amount"/> is signed.</summary>
public sealed record ForecastItem(string Label, ForecastSource Source, decimal Amount, int Count, DateOnly FirstDate);

/// <param name="Start">Balance today (completed and pending records up to today).</param>
/// <param name="Points">Balance at the end of each day or week, starting today.</param>
/// <param name="Trend">Null when the trend is off.</param>
public sealed record ForecastData(
    string Currency,
    int AccountCount,
    decimal Start,
    IReadOnlyList<ForecastPoint> Points,
    decimal End,
    ForecastPoint Lowest,
    IReadOnlyList<ForecastItem> Items,
    int RecurringCount,
    int RecordCount,
    ForecastTrend? Trend,
    IReadOnlyList<ForecastMonth> Months);
