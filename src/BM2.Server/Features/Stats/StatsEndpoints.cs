using BM2.Contracts.Accounts;
using BM2.Contracts.Categories;
using BM2.Contracts.Records;
using BM2.Contracts.Stats;
using BM2.Domain.Common;
using BM2.Domain.Records;
using BM2.Server.Data;
using BM2.Server.Features.Records;
using BM2.Server.Features.Wallets;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Stats;

public static class StatsEndpoints
{
    private const int MaxMonths = 24;

    public static RouteGroupBuilder MapStatsEndpoints(this RouteGroupBuilder wallet)
    {
        var stats = wallet.MapGroup("/stats");
        stats.MapGet("/categories", CategoriesAsync);
        stats.MapGet("/monthly", MonthlyAsync);
        stats.MapGet("/category-months", CategoryMonthsAsync);
        GetForecast.Map(stats);
        return wallet;
    }

    private static async Task<Results<Ok<CategoryTotals>, ProblemHttpResult>> CategoriesAsync(
        [AsParameters] StatsFilter filter, CurrentWallet current, AppDbContext db, CancellationToken ct)
    {
        var currency = await CurrencyAsync(filter, current, db, ct);
        if (Query(db, current, filter, currency) is not { } query)
        {
            return Error.NotFound().ToProblem();
        }

        var items = await query
            .GroupBy(r => new { r.CategoryId, r.Type })
            .Select(g => new CategoryTotal(g.Key.CategoryId, g.Key.Type, g.Sum(r => r.Amount), g.Count()))
            .ToListAsync(ct);

        return TypedResults.Ok(new CategoryTotals(currency, items.OrderByDescending(i => i.Total).ToList()));
    }

    /// <summary>Months from <c>From</c> up to <c>To</c> (both month starts); defaults to the 12 months ending with the current one.</summary>
    private static async Task<Results<Ok<MonthlyTotals>, ProblemHttpResult>> MonthlyAsync(
        [AsParameters] StatsFilter filter, CurrentWallet current, AppDbContext db, TimeProvider time, CancellationToken ct)
    {
        var (from, to) = MonthRange(filter, time);
        var currency = await CurrencyAsync(filter, current, db, ct);
        if (Query(db, current, filter, currency, from, to) is not { } query)
        {
            return Error.NotFound().ToProblem();
        }

        // Grouped by day in SQL (2012-safe), folded into months in memory: at most a few hundred rows.
        var days = await query
            .GroupBy(r => new { r.OccurredOn, r.Type })
            .Select(g => new { g.Key.OccurredOn, g.Key.Type, Total = g.Sum(r => r.Amount) })
            .ToListAsync(ct);

        // Transfers between cash and investment accounts, on the side in the stats currency.
        var invested = await Base(db, current, filter, from, to)!
            .Where(r => r.Type == RecordType.Transfer && r.Account.Kind != r.TransferAccount!.Kind)
            .Select(r => new
            {
                r.OccurredOn,
                Amount = r.TransferAccount!.Kind == AccountKind.Investment
                    ? (r.Account.Currency == currency ? r.AccountAmount : 0)
                    : (r.TransferAccount.Currency == currency ? -(r.TransferAmount ?? r.AccountAmount) : 0),
            })
            .GroupBy(r => r.OccurredOn)
            .Select(g => new { OccurredOn = g.Key, Total = g.Sum(r => r.Amount) })
            .ToListAsync(ct);

        var items = new List<MonthTotal>();
        for (var month = from; month < to; month = month.AddMonths(1))
        {
            var end = month.AddMonths(1);
            var inMonth = days.Where(d => d.OccurredOn >= month && d.OccurredOn < end).ToList();
            items.Add(new MonthTotal(
                month,
                inMonth.Where(d => d.Type == RecordType.Income).Sum(d => d.Total),
                inMonth.Where(d => d.Type == RecordType.Expense).Sum(d => d.Total),
                invested.Where(d => d.OccurredOn >= month && d.OccurredOn < end).Sum(d => d.Total)));
        }

        return TypedResults.Ok(new MonthlyTotals(currency, items));
    }

    /// <summary>Per category and month, for averages over chosen months; same range rules as monthly.</summary>
    private static async Task<Results<Ok<CategoryMonthTotals>, ProblemHttpResult>> CategoryMonthsAsync(
        [AsParameters] StatsFilter filter, CurrentWallet current, AppDbContext db, TimeProvider time, CancellationToken ct)
    {
        var (from, to) = MonthRange(filter, time);
        var currency = await CurrencyAsync(filter, current, db, ct);
        if (Query(db, current, filter, currency, from, to) is not { } query)
        {
            return Error.NotFound().ToProblem();
        }

        var days = await query
            .GroupBy(r => new { r.CategoryId, r.Type, r.OccurredOn })
            .Select(g => new { g.Key.CategoryId, g.Key.Type, g.Key.OccurredOn, Total = g.Sum(r => r.Amount) })
            .ToListAsync(ct);

        var items = days
            .GroupBy(d => (d.CategoryId, d.Type, Month: MonthStart(d.OccurredOn)))
            .Select(g => new CategoryMonthTotal(g.Key.CategoryId, g.Key.Type, g.Key.Month, g.Sum(d => d.Total)))
            .OrderBy(i => i.Month)
            .ToList();

        return TypedResults.Ok(new CategoryMonthTotals(currency, items));
    }

    // Month starts; defaults to the 12 months ending with the current one, at most MaxMonths long.
    private static (DateOnly From, DateOnly To) MonthRange(StatsFilter filter, TimeProvider time)
    {
        var today = DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime);
        var to = MonthStart(filter.To ?? MonthStart(today).AddMonths(1));
        var from = MonthStart(filter.From ?? to.AddMonths(-12));
        return from >= to || from < to.AddMonths(-MaxMonths) ? (to.AddMonths(-12), to) : (from, to);
    }

    private static DateOnly MonthStart(DateOnly day) => new(day.Year, day.Month, 1);

    private static async Task<string> CurrencyAsync(StatsFilter filter, CurrentWallet current, AppDbContext db, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(filter.Currency))
        {
            return filter.Currency.Trim().ToUpperInvariant();
        }

        var walletId = current.Id;
        return await db.Wallets.Where(w => w.Id == walletId).Select(w => w.BaseCurrency).SingleAsync(ct);
    }

    // Completed and pending income and expenses on visible accounts in one currency; null when the account is not readable.
    // Refunds (income in an expense category) count as negative expenses.
    private static IQueryable<StatRow>? Query(
        AppDbContext db, CurrentWallet current, StatsFilter filter, string currency, DateOnly? from = null, DateOnly? to = null) =>
        Base(db, current, filter, from, to)?
            .Where(r => r.Type != RecordType.Transfer && r.Account.Currency == currency)
            .Select(r => new StatRow
            {
                CategoryId = r.CategoryId,
                OccurredOn = r.OccurredOn,
                Type = r.Type == RecordType.Income && r.Category!.Type == CategoryType.Expense ? RecordType.Expense : r.Type,
                Amount = r.Type == RecordType.Income && r.Category!.Type == CategoryType.Expense ? -r.AccountAmount : r.AccountAmount,
            });

    // Completed and pending records matching the filter; null when the account is not readable.
    private static IQueryable<Record>? Base(
        AppDbContext db, CurrentWallet current, StatsFilter filter, DateOnly? from = null, DateOnly? to = null)
    {
        var query = RecordQueries.Filtered(db, current, new RecordFilter
        {
            From = from ?? filter.From,
            To = to ?? filter.To,
            AccountId = filter.AccountId,
            CategoryId = filter.CategoryId,
        })?.Where(r => r.Status != RecordStatus.Planned);

        if (query is not null && filter.TagIds is { Length: > 0 } tagIds)
        {
            query = query.Where(r => r.Tags.Any(t => tagIds.Contains(t.TagId)));
        }

        return query;
    }

    private sealed class StatRow
    {
        public Guid? CategoryId { get; init; }
        public DateOnly OccurredOn { get; init; }
        public RecordType Type { get; init; }
        public decimal Amount { get; init; }
    }
}
