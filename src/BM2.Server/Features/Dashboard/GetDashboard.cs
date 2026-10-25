using BM2.Contracts.Dashboard;
using BM2.Contracts.Records;
using BM2.Server.Data;
using BM2.Server.Features.Budgets;
using BM2.Server.Features.Records;
using BM2.Server.Features.Recurring;
using BM2.Server.Features.Templates;
using BM2.Server.Features.Wallets;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BM2.Server.Features.Dashboard;

internal static class GetDashboard
{
    public static RouteGroupBuilder MapDashboardEndpoints(this RouteGroupBuilder wallet)
    {
        wallet.MapGet("/dashboard", HandleAsync);
        return wallet;
    }

    /// <param name="today">The device's date, so the month matches the user's time zone.</param>
    private static async Task<Ok<DashboardData>> HandleAsync(
        CurrentWallet current, AppDbContext db, TimeProvider time, CancellationToken ct, DateOnly? today = null)
    {
        var day = today ?? DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime);
        var start = new DateOnly(day.Year, day.Month, 1);

        // One DbContext, so the queries run one after another.
        var month = await GetRecordSummary.SummarizeAsync(
            RecordQueries.Filtered(db, current, new RecordFilter { From = start, To = start.AddMonths(1), Status = RecordStatus.Completed })!, ct);
        var pending = await ListRecords.LatestAsync(db, RecordQueries.Filtered(db, current, new RecordFilter { Status = RecordStatus.Pending })!, 10, ct);
        var planned = await ListRecords.EarliestAsync(db, RecordQueries.Filtered(db, current, new RecordFilter { Status = RecordStatus.Planned })!, 10, ct);
        var templates =(await ListTemplates.LoadAsync(current, db, ct)).Where(t => t.IsPinned).ToList();
        var budgets = await BudgetEndpoints.LoadAsync(current, db, day, ct);
        var recurring = await RecurringEndpoints.LoadAsync(current, db, ct);
        var recent = await ListRecords.LatestAsync(db, RecordQueries.Filtered(db, current, new RecordFilter())!, 5, ct);

        return TypedResults.Ok(new DashboardData(month, pending, planned, templates, budgets, recurring, recent));
    }
}
