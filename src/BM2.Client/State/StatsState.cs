using BM2.Contracts.Records;
using BM2.Contracts.Stats;
using BM2.Contracts.Wallets;

namespace BM2.Client.State;

public enum StatsPeriod { Month, Year, Custom }

/// <summary>Period: month, year or custom range; Months: 12 months up to a month; Year: a whole year.</summary>
public enum StatsFilterMode { Period, Months, Year }

/// <summary>Stats filters shared by the stats screens, so they survive moving between them.</summary>
public sealed class StatsState
{
    private Guid? _walletId;

    public StatsPeriod Period { get; set; } = StatsPeriod.Month;

    /// <summary>First day of the selected month (its year in year mode).</summary>
    public DateOnly Anchor { get; set; } = new(DateTime.Now.Year, DateTime.Now.Month, 1);

    public DateOnly From { get; set; } = new(DateTime.Now.Year, DateTime.Now.Month, 1);

    /// <summary>Inclusive end of a custom period.</summary>
    public DateOnly To { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    public Guid? AccountId { get; set; }

    public string? Currency { get; set; }

    public RecordType Type { get; set; } = RecordType.Expense;

    public IReadOnlyCollection<Guid> TagIds { get; set; } = [];

    public Guid? TagCategoryId { get; set; }

    public int ForecastMonths { get; set; } = 6;

    public ForecastStep ForecastStep { get; set; } = ForecastStep.Week;

    public bool ForecastTrend { get; set; } = true;

    /// <summary>Months (1–12) of the anchor year counted in averages; null means the default.</summary>
    public IReadOnlyCollection<int>? AverageMonths { get; set; }

    /// <summary>Exclusive range of the selected period.</summary>
    public (DateOnly From, DateOnly To) Range => Period switch
    {
        StatsPeriod.Month => (Anchor, Anchor.AddMonths(1)),
        StatsPeriod.Year => (new DateOnly(Anchor.Year, 1, 1), new DateOnly(Anchor.Year + 1, 1, 1)),
        _ => (From, To.AddDays(1)),
    };

    public bool IsValid => Period != StatsPeriod.Custom || From <= To;

    /// <summary>The 12 months ending with the period's last month.</summary>
    public (DateOnly From, DateOnly To) TrendRange
    {
        get
        {
            var last = Range.To.AddDays(-1);
            return MonthsEndingWith(new DateOnly(last.Year, last.Month, 1));
        }
    }

    public static (DateOnly From, DateOnly To) MonthsEndingWith(DateOnly month) => (month.AddMonths(-11), month.AddMonths(1));

    /// <summary>Resets the filters when another wallet is shown.</summary>
    public void Use(WalletSnapshot wallet)
    {
        if (_walletId == wallet.Id)
        {
            return;
        }

        _walletId = wallet.Id;
        Currency = wallet.BaseCurrency;
        AccountId = null;
        TagIds = [];
        TagCategoryId = null;
    }

    public string Url(Guid walletId, string endpoint, DateOnly from, DateOnly to, Guid? categoryId = null, IEnumerable<Guid>? tagIds = null) =>
        $"api/wallets/{walletId}/stats/{endpoint}" + new StatsFilter
        {
            From = from,
            To = to,
            AccountId = AccountId,
            Currency = Currency,
            CategoryId = categoryId,
            TagIds = tagIds?.ToArray(),
        }.ToQueryString();
}
