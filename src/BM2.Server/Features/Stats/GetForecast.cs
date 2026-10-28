using BM2.Contracts.Accounts;
using BM2.Contracts.Categories;
using BM2.Contracts.Records;
using BM2.Contracts.Stats;
using BM2.Domain.Stats;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Stats;

/// <summary>
/// Balance forecast for the readable accounts in one currency: today's balance, then recurring occurrences not added
/// yet, dated records after today (and overdue planned ones, counted today) and optionally the income and spending
/// trend (<see cref="MonthlyTrend"/>) of up to 24 full months, spread evenly over each month's days.
/// </summary>
internal static class GetForecast
{
    private const int MaxRecords = 5000;
    private const int TopItems = 8;
    private const int HistoryMonths = 24;
    // ~80% two-sided band of a normal error.
    private const decimal Z = 1.28m;

    public static void Map(RouteGroupBuilder stats) => stats.MapGet("/forecast", HandleAsync);

    private static async Task<Ok<ForecastData>> HandleAsync(
        [AsParameters] ForecastQuery query, CurrentWallet current, AppDbContext db, TimeProvider time, CancellationToken ct)
    {
        var access = current.Access;
        var walletId = current.Id;
        var today = query.Today ?? DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime);
        var end = today.AddMonths(query.Months is 3 or 6 or 12 or 24 ? query.Months.Value : 6);
        var currency = string.IsNullOrWhiteSpace(query.Currency)
            ? await db.Wallets.Where(w => w.Id == walletId).Select(w => w.BaseCurrency).SingleAsync(ct)
            : query.Currency.Trim().ToUpperInvariant();

        var accounts = (await db.Accounts
                .Where(a => a.WalletId == walletId && a.Currency == currency && a.Kind == AccountKind.Cash && !a.ExcludeFromTotal)
                .Select(a => new { a.Id, a.OpeningBalance })
                .ToListAsync(ct))
            .Where(a => access.CanReadAccount(a.Id))
            .ToList();
        var ids = accounts.Select(a => a.Id).ToList();
        var inSet = ids.ToHashSet();

        var touching = db.Records.AsNoTracking()
            .Where(r => ids.Contains(r.AccountId) || (r.TransferAccountId != null && ids.Contains(r.TransferAccountId.Value)));

        var past = touching.Where(r => r.Status != RecordStatus.Planned && r.OccurredOn <= today);
        var own = await past.Where(r => ids.Contains(r.AccountId))
            .SumAsync(r => r.Type == RecordType.Income ? r.AccountAmount : -r.AccountAmount, ct);
        var incoming = await past.Where(r => r.TransferAccountId != null && ids.Contains(r.TransferAccountId.Value))
            .SumAsync(r => r.TransferAmount ?? 0, ct);
        var start = accounts.Sum(a => a.OpeningBalance) + own + incoming;

        var categories = await db.Categories.Where(c => c.WalletId == walletId).ToDictionaryAsync(c => c.Id, c => c.Name, ct);
        string Label(string? name, Guid? categoryId, RecordType type) =>
            name ?? (categoryId is { } id && categories.TryGetValue(id, out var category) ? category : type.ToString());
        decimal Delta(RecordType type, Guid accountId, decimal accountAmount, Guid? transferAccountId, decimal? transferAmount) =>
            (inSet.Contains(accountId) ? (type == RecordType.Income ? accountAmount : -accountAmount) : 0)
            + (transferAccountId is { } target && inSet.Contains(target) ? transferAmount ?? 0 : 0);

        // Events by day; items group a rule's occurrences into one line.
        var byDay = new Dictionary<DateOnly, (decimal In, decimal Out)>();
        var items = new Dictionary<string, (string Label, ForecastSource Source, decimal Amount, int Count, DateOnly First)>();
        void Add(string key, string label, ForecastSource source, DateOnly date, decimal amount)
        {
            if (amount == 0)
            {
                return;
            }

            date = date < today ? today : date;
            var (inflow, outflow) = byDay.GetValueOrDefault(date);
            byDay[date] = (inflow + Math.Max(amount, 0), outflow + Math.Max(-amount, 0));
            items[key] = items.TryGetValue(key, out var item)
                ? item with { Amount = item.Amount + amount, Count = item.Count + 1, First = date < item.First ? date : item.First }
                : (label, source, amount, 1, date);
        }

        var records = await touching
            .Where(r => (r.OccurredOn > today && r.OccurredOn <= end) || (r.Status == RecordStatus.Planned && r.OccurredOn <= today))
            .OrderBy(r => r.OccurredOn)
            .Select(r => new { r.Id, r.Type, r.Status, r.OccurredOn, r.AccountId, r.AccountAmount, r.TransferAccountId, r.TransferAmount, r.Name, r.CategoryId })
            .Take(MaxRecords)
            .ToListAsync(ct);
        foreach (var r in records)
        {
            var source = r.Status switch
            {
                RecordStatus.Planned => ForecastSource.Planned,
                RecordStatus.Pending => ForecastSource.Pending,
                _ => ForecastSource.Completed,
            };
            Add(r.Id.ToString(), Label(r.Name, r.CategoryId, r.Type), source, r.OccurredOn,
                Delta(r.Type, r.AccountId, r.AccountAmount, r.TransferAccountId, r.TransferAmount));
        }

        var rules = (await db.RecurringRules.AsNoTracking()
                .Where(r => r.WalletId == walletId && !r.IsPaused && r.NextDate != null)
                .ToListAsync(ct))
            .Where(r => access.CanReadRecord(r.AccountId, r.TransferAccountId)
                && (inSet.Contains(r.AccountId) || (r.TransferAccountId is { } t && inSet.Contains(t))))
            .ToList();
        foreach (var rule in rules)
        {
            var schedule = rule.Schedule;
            for (var index = rule.NextIndex; schedule.NextDate(index) is { } date && date <= end; index++)
            {
                Add("rule:" + rule.Id, Label(rule.Name, rule.CategoryId, rule.Type), ForecastSource.Recurring, date,
                    Delta(rule.Type, rule.AccountId, rule.AccountAmount, rule.TransferAccountId, rule.TransferAmount));
            }
        }

        // Rules and dated records are added above, so the trend history leaves out records made by rules and every
        // category an active rule uses (expense rules on the spending side, income rules on the income side).
        // shortcut: one-off planned or future-dated records still overlap the trend a little; fine for an estimate.
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        MonthlyTrend? income = null, spending = null;
        ForecastTrend? trend = null;
        decimal sigma = 0;
        if (query.Trend ?? true)
        {
            var coveredExpense = rules.Where(r => r.Type == RecordType.Expense && r.CategoryId != null).Select(r => r.CategoryId!.Value).Distinct().ToList();
            var coveredIncome = rules.Where(r => r.Type == RecordType.Income && r.CategoryId != null).Select(r => r.CategoryId!.Value).Distinct().ToList();

            // History starts with the first full month that has records, at most 24 months back.
            var first = monthStart.AddMonths(-HistoryMonths);
            var earliest = await db.Records
                .Where(r => ids.Contains(r.AccountId) && r.Status != RecordStatus.Planned && r.OccurredOn >= first && r.OccurredOn < monthStart)
                .MinAsync(r => (DateOnly?)r.OccurredOn, ct);
            first = earliest is { } e ? new DateOnly(e.Year, e.Month, 1).AddMonths(e.Day > 1 ? 1 : 0) : monthStart;
            var n = Math.Max(0, MonthsBetween(first, monthStart));

            var totals = await db.Records.AsNoTracking()
                .Where(r => ids.Contains(r.AccountId) && r.Status != RecordStatus.Planned && r.RecurringRuleId == null
                    && r.Type != RecordType.Transfer && r.OccurredOn >= first && r.OccurredOn < monthStart)
                .Select(r => new
                {
                    r.OccurredOn.Year,
                    r.OccurredOn.Month,
                    Spending = r.Type == RecordType.Expense || r.Category!.Type == CategoryType.Expense,
                    r.CategoryId,
                    Amount = r.Type == RecordType.Income ? r.AccountAmount : -r.AccountAmount,
                })
                .Where(x => x.CategoryId == null
                    || (x.Spending && !coveredExpense.Contains(x.CategoryId.Value))
                    || (!x.Spending && !coveredIncome.Contains(x.CategoryId.Value)))
                .GroupBy(x => new { x.Year, x.Month, x.Spending })
                .Select(g => new { g.Key.Year, g.Key.Month, g.Key.Spending, Total = g.Sum(x => x.Amount) })
                .ToListAsync(ct);
            var incomes = new decimal[n];
            var expenses = new decimal[n];
            foreach (var total in totals)
            {
                var t = MonthsBetween(first, new DateOnly(total.Year, total.Month, 1));
                if (total.Spending)
                {
                    expenses[t] -= total.Total;
                }
                else
                {
                    incomes[t] += total.Total;
                }
            }

            income = MonthlyTrend.Fit(first, incomes);
            spending = MonthlyTrend.Fit(first, expenses);
            sigma = (decimal)Math.Sqrt((double)(income.Sigma * income.Sigma + spending.Sigma * spending.Sigma));
            trend = new ForecastTrend(income.Model, first, n,
                Math.Round(income.Predict(monthStart), 2), Math.Round(income.Slope, 2),
                Math.Round(spending.Predict(monthStart), 2), Math.Round(spending.Slope, 2),
                Math.Round(Z * sigma, 2), coveredExpense.Count + coveredIncome.Count);
        }

        // Independent monthly errors add up, so the band grows with the square root of the months ahead.
        decimal Spread(DateOnly day) =>
            Math.Round(Z * sigma * (decimal)Math.Sqrt((day.DayNumber - today.DayNumber) / 30.4375), 2);

        var step = query.Step ?? ForecastStep.Week;
        var points = new List<ForecastPoint>();
        var months = new List<ForecastMonth>();
        decimal balance = start, monthIn = 0, monthOut = 0;
        var lowest = new ForecastPoint(today, decimal.MaxValue);
        for (var day = today; day <= end; day = day.AddDays(1))
        {
            var (inflow, outflow) = byDay.GetValueOrDefault(day);
            if (day > today && income is not null && spending is not null)
            {
                var month = new DateOnly(day.Year, day.Month, 1);
                var days = DateTime.DaysInMonth(day.Year, day.Month);
                inflow += income.Predict(month) / days;
                outflow += spending.Predict(month) / days;
            }

            balance += inflow - outflow;
            monthIn += inflow;
            monthOut += outflow;
            var rounded = Math.Round(balance, 2);
            if (rounded < lowest.Balance)
            {
                lowest = new ForecastPoint(day, rounded);
            }

            if (step == ForecastStep.Day || (day.DayNumber - today.DayNumber) % 7 == 0 || day == end)
            {
                points.Add(new ForecastPoint(day, rounded, Spread(day)));
            }

            if (day == end || day.AddDays(1).Day == 1)
            {
                months.Add(new ForecastMonth(new DateOnly(day.Year, day.Month, 1), Math.Round(monthIn, 2), Math.Round(monthOut, 2), rounded, Spread(day)));
                monthIn = monthOut = 0;
            }
        }

        var top = items.Values
            .OrderByDescending(i => Math.Abs(i.Amount))
            .Take(TopItems)
            .Select(i => new ForecastItem(i.Label, i.Source, i.Amount, i.Count, i.First))
            .ToList();

        return TypedResults.Ok(new ForecastData(
            currency, ids.Count, start, points, Math.Round(balance, 2), lowest, top,
            rules.Count, records.Count, trend, months));
    }

    private static int MonthsBetween(DateOnly from, DateOnly to) => (to.Year - from.Year) * 12 + to.Month - from.Month;
}
