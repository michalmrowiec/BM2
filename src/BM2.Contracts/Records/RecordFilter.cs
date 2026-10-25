using System.Globalization;

namespace BM2.Contracts.Records;

/// <summary>Record list query; <see cref="To"/> is exclusive, no dates means all time.</summary>
public sealed class RecordFilter
{
    public DateOnly? From { get; init; }

    public DateOnly? To { get; init; }

    public Guid? AccountId { get; init; }

    public Guid? CategoryId { get; init; }

    public Guid? TagId { get; init; }

    public RecordType? Type { get; init; }

    public RecordStatus? Status { get; init; }

    public string? Search { get; init; }

    /// <summary>Only records added by this recurring rule.</summary>
    public Guid? RuleId { get; init; }

    /// <summary>True: only records added by recurring rules.</summary>
    public bool? Recurring { get; init; }

    public string? Cursor { get; init; }

    /// <summary>Page size, 1–50 (default 50).</summary>
    public int? Limit { get; init; }

    public string ToQueryString()
    {
        var parts = new List<string>();
        Add(parts, "from", From?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        Add(parts, "to", To?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        Add(parts, "accountId", AccountId?.ToString());
        Add(parts, "categoryId", CategoryId?.ToString());
        Add(parts, "tagId", TagId?.ToString());
        Add(parts, "type", Type?.ToString());
        Add(parts, "status", Status?.ToString());
        Add(parts, "search", string.IsNullOrWhiteSpace(Search) ? null : Search.Trim());
        Add(parts, "ruleId", RuleId?.ToString());
        Add(parts, "recurring", Recurring == true ? "true" : null);
        Add(parts, "cursor", Cursor);
        Add(parts, "limit", Limit?.ToString(CultureInfo.InvariantCulture));
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

public sealed record RecordPage(IReadOnlyList<RecordItem> Items, string? NextCursor);

public sealed record CurrencyTotals(string Currency, decimal Income, decimal Expense);

/// <summary>Income and expense per account currency; transfers are not counted.</summary>
public sealed record RecordSummary(IReadOnlyList<CurrencyTotals> Totals);
