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
            .Select(g => new CategoryTotal(g.Key.CategoryId, g.Key.Type, g.Sum(r => r.AccountAmount), g.Count()))
            .ToListAsync(ct);

        return TypedResults.Ok(new CategoryTotals(currency, items.OrderByDescending(i => i.Total).ToList()));
    }

    /// <summary>Months from <c>From</c> up to <c>To</c> (both month starts); defaults to the 12 months ending with the current one.</summary>
    private static async Task<Results<Ok<MonthlyTotals>, ProblemHttpResult>> MonthlyAsync(
        [AsParameters] StatsFilter filter, CurrentWallet current, AppDbContext db, TimeProvider time, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime);
        var to = MonthStart(filter.To ?? new DateOnly(today.Year, today.Month, 1).AddMonths(1));
        var from = MonthStart(filter.From ?? to.AddMonths(-12));
        if (from >= to || from < to.AddMonths(-MaxMonths))
        {
            from = to.AddMonths(-12);
        }

        var currency = await CurrencyAsync(filter, current, db, ct);
        var range = new StatsFilter
        {
            From = from, To = to, AccountId = filter.AccountId, CategoryId = filter.CategoryId, TagId = filter.TagId,
        };
        if (Query(db, current, range, currency) is not { } query)
        {
            return Error.NotFound().ToProblem();
        }

        // Grouped by day in SQL (2012-safe), folded into months in memory: at most a few hundred rows.
        var days = await query
            .GroupBy(r => new { r.OccurredOn, r.Type })
            .Select(g => new { g.Key.OccurredOn, g.Key.Type, Total = g.Sum(r => r.AccountAmount) })
            .ToListAsync(ct);

        var items = new List<MonthTotal>();
        for (var month = from; month < to; month = month.AddMonths(1))
        {
            var end = month.AddMonths(1);
            var inMonth = days.Where(d => d.OccurredOn >= month && d.OccurredOn < end).ToList();
            items.Add(new MonthTotal(
                month,
                inMonth.Where(d => d.Type == RecordType.Income).Sum(d => d.Total),
                inMonth.Where(d => d.Type == RecordType.Expense).Sum(d => d.Total)));
        }

        return TypedResults.Ok(new MonthlyTotals(currency, items));
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

    // Completed income and expenses on visible accounts in one currency; null when the account is not readable.
    private static IQueryable<Record>? Query(AppDbContext db, CurrentWallet current, StatsFilter filter, string currency) =>
        RecordQueries.Filtered(db, current, new RecordFilter
        {
            From = filter.From,
            To = filter.To,
            AccountId = filter.AccountId,
            CategoryId = filter.CategoryId,
            TagId = filter.TagId,
            Status = RecordStatus.Completed,
        })?.Where(r => r.Type != RecordType.Transfer && r.Account.Currency == currency);
}
