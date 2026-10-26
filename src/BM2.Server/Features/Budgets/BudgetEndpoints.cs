using BM2.Contracts.Budgets;
using BM2.Contracts.Categories;
using BM2.Contracts.Records;
using BM2.Domain.Access;
using BM2.Domain.Budgets;
using BM2.Domain.Common;
using BM2.Server.Data;
using BM2.Server.Features.Records;
using BM2.Server.Features.Wallets;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Budgets;

public static class BudgetEndpoints
{
    public static RouteGroupBuilder MapBudgetEndpoints(this RouteGroupBuilder wallet)
    {
        var budgets = wallet.MapGroup("/budgets");
        budgets.MapGet("/", ListAsync);
        budgets.MapPut("/", SaveAsync).Validate<SaveBudgetRequest>();
        budgets.MapDelete("/{budgetId:guid}", DeleteAsync);
        return wallet;
    }

    /// <param name="month">Any day of the month; defaults to the server's current month.</param>
    private static async Task<Ok<BudgetOverview>> ListAsync(
        CurrentWallet current, AppDbContext db, TimeProvider time, CancellationToken ct, DateOnly? month = null) =>
        TypedResults.Ok(await LoadAsync(current, db, month ?? DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime), ct));

    internal static async Task<BudgetOverview> LoadAsync(CurrentWallet current, AppDbContext db, DateOnly day, CancellationToken ct)
    {
        var walletId = current.Id;
        var start = new DateOnly(day.Year, day.Month, 1);
        var end = start.AddMonths(1);
        var currency = await db.Wallets.Where(w => w.Id == walletId).Select(w => w.BaseCurrency).SingleAsync(ct);
        var budgets = await db.Budgets
            .AsNoTracking()
            .Where(b => b.WalletId == walletId)
            .Select(b => new { b.Id, b.CategoryId, b.Limit })
            .ToListAsync(ct);
        if (budgets.Count == 0)
        {
            return new BudgetOverview(start, currency, []);
        }

        var categoryIds = budgets.Select(b => b.CategoryId).ToList();
        var spent = await db.Records
            .AsNoTracking()
            .Visible(current.Access.ReadableAccountIds)
            .Where(r => r.Type != RecordType.Transfer
                && r.OccurredOn >= start && r.OccurredOn < end
                && r.CategoryId != null && categoryIds.Contains(r.CategoryId.Value))
            .GroupBy(r => new { CategoryId = r.CategoryId!.Value, r.Account.Currency, Planned = r.Status == RecordStatus.Planned })
            .Select(g => new { g.Key.CategoryId, g.Key.Currency, g.Key.Planned, Amount = g.Sum(r => r.Type == RecordType.Income ? -r.AccountAmount : r.AccountAmount) })
            .ToListAsync(ct);
        var planned = spent.Where(s => s.Planned).ToList();
        spent = spent.Where(s => !s.Planned).ToList();

        return new BudgetOverview(start, currency, budgets
            .Select(b => new BudgetItem(
                b.Id,
                b.CategoryId,
                b.Limit,
                spent.Where(s => s.CategoryId == b.CategoryId && s.Currency == currency).Sum(s => s.Amount),
                planned.Where(s => s.CategoryId == b.CategoryId && s.Currency == currency).Sum(s => s.Amount),
                spent.Where(s => s.CategoryId == b.CategoryId && s.Currency != currency)
                    .OrderBy(s => s.Currency)
                    .Select(s => new CurrencyAmount(s.Currency, s.Amount))
                    .ToList()))
            .ToList());
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> SaveAsync(
        SaveBudgetRequest request, CurrentWallet current, AppDbContext db, CancellationToken ct)
    {
        if (!await db.Categories.AnyAsync(
                c => c.Id == request.CategoryId && c.WalletId == current.Id && c.Type == CategoryType.Expense, ct))
        {
            return Error.NotFound().ToProblem();
        }

        if (!current.Access.Can(WalletPermission.ManageCategories))
        {
            return Error.Forbidden().ToProblem();
        }

        var budget = await db.Budgets.SingleOrDefaultAsync(b => b.WalletId == current.Id && b.CategoryId == request.CategoryId, ct);
        if (budget is null)
        {
            db.Budgets.Add(Budget.Create(current.Id, request.CategoryId, request.Limit));
        }
        else
        {
            budget.SetLimit(request.Limit);
        }

        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteAsync(
        Guid budgetId, CurrentWallet current, AppDbContext db, CancellationToken ct)
    {
        if (!await db.Budgets.AnyAsync(b => b.Id == budgetId && b.WalletId == current.Id, ct))
        {
            return Error.NotFound().ToProblem();
        }

        if (!current.Access.Can(WalletPermission.ManageCategories))
        {
            return Error.Forbidden().ToProblem();
        }

        await db.Budgets.Where(b => b.Id == budgetId).ExecuteDeleteAsync(ct);
        return TypedResults.NoContent();
    }
}
