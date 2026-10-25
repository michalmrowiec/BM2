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

    public Guid? TagId { get; init; }

    public string ToQueryString()
    {
        var parts = new List<string>();
        Add(parts, "from", From?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        Add(parts, "to", To?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        Add(parts, "accountId", AccountId?.ToString());
        Add(parts, "currency", Currency);
        Add(parts, "categoryId", CategoryId?.ToString());
        Add(parts, "tagId", TagId?.ToString());
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
public sealed record MonthTotal(DateOnly Month, decimal Income, decimal Expense);

/// <summary>Every month of the range, oldest first, including empty ones.</summary>
public sealed record MonthlyTotals(string Currency, IReadOnlyList<MonthTotal> Items);
