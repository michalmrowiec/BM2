using BM2.Contracts.Records;
using BM2.Contracts.Stats;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Stats;

/// <summary>
/// Balance forecast for the readable accounts in one currency: today's balance, then recurring occurrences not added
/// yet, dated records after today (and overdue planned ones, counted today) and optionally the average daily
/// variable spending of the last 3 full months from categories no recurring expense covers.
/// </summary>
internal static class GetForecast
{
    private const int MaxRecords = 5000;
    private const int TopItems = 8;

    public static void Map(RouteGroupBuilder stats) => stats.MapGet("/forecast", HandleAsync);

    private static async Task<Ok<ForecastData>> HandleAsync(
        [AsParameters] ForecastQuery query, CurrentWallet current, AppDbContext db, TimeProvider time, CancellationToken ct)
    {
        var access = current.Access;
        var walletId = current.Id;
        var today = query.Today ?? DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime);
        var end = today.AddMonths(query.Months is 3 or 6 or 12 ? query.Months.Value : 6);
        var currency = string.IsNullOrWhiteSpace(query.Currency)
            ? await db.Wallets.Where(w => w.Id == walletId).Select(w => w.BaseCurrency).SingleAsync(ct)
            : query.Currency.Trim().ToUpperInvariant();

        var accounts = (await db.Accounts
                .Where(a => a.WalletId == walletId && a.Currency == currency)
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
        var byDay = new Dictionary<DateOnly, decimal>();
        var items = new Dictionary<string, (string Label, ForecastSource Source, decimal Amount, int Count, DateOnly First)>();
        void Add(string key, string label, ForecastSource source, DateOnly date, decimal amount)
        {
            if (amount == 0)
            {
                return;
            }

            date = date < today ? today : date;
            byDay[date] = byDay.GetValueOrDefault(date) + amount;
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

        // Average variable spending: expenses not added by rules, from categories no active recurring expense uses.
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var averageFrom = monthStart.AddMonths(-3);
        decimal dailyAverage = 0;
        var averagedCategories = 0;
        if (query.Averages ?? true)
        {
            var covered = rules.Where(r => r.Type == RecordType.Expense && r.CategoryId != null).Select(r => r.CategoryId!.Value).Distinct().ToList();
            var spent = await db.Records.AsNoTracking()
                .Where(r => ids.Contains(r.AccountId) && r.Type == RecordType.Expense && r.Status != RecordStatus.Planned
                    && r.RecurringRuleId == null && r.OccurredOn >= averageFrom && r.OccurredOn < monthStart
                    && (r.CategoryId == null || !covered.Contains(r.CategoryId.Value)))
                .GroupBy(r => r.CategoryId)
                .Select(g => g.Sum(r => r.AccountAmount))
                .ToListAsync(ct);
            averagedCategories = spent.Count;
            dailyAverage = Math.Round(spent.Sum() / (monthStart.DayNumber - averageFrom.DayNumber), 2);
        }

        var step = query.Step ?? ForecastStep.Week;
        var points = new List<ForecastPoint>();
        var balance = start;
        var lowest = new ForecastPoint(today, decimal.MaxValue);
        for (var day = today; day <= end; day = day.AddDays(1))
        {
            balance += byDay.GetValueOrDefault(day) - (day > today ? dailyAverage : 0);
            if (balance < lowest.Balance)
            {
                lowest = new ForecastPoint(day, balance);
            }

            if (step == ForecastStep.Day || (day.DayNumber - today.DayNumber) % 7 == 0 || day == end)
            {
                points.Add(new ForecastPoint(day, balance));
            }
        }

        var top = items.Values
            .OrderByDescending(i => Math.Abs(i.Amount))
            .Take(TopItems)
            .Select(i => new ForecastItem(i.Label, i.Source, i.Amount, i.Count, i.First))
            .ToList();

        return TypedResults.Ok(new ForecastData(
            currency, ids.Count, start, points, balance, lowest, top,
            rules.Count, records.Count, dailyAverage, averageFrom, averagedCategories));
    }
}
