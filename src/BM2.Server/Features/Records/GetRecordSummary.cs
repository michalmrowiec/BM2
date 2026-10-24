using BM2.Contracts.Records;
using BM2.Domain.Common;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Records;

internal static class GetRecordSummary
{
    public static void Map(RouteGroupBuilder records) => records.MapGet("/summary", HandleAsync);

    private static async Task<Results<Ok<RecordSummary>, ProblemHttpResult>> HandleAsync(
        [AsParameters] RecordFilter filter, CurrentWallet current, AppDbContext db, CancellationToken ct)
    {
        if (RecordQueries.Filtered(db, current, filter) is not { } query)
        {
            return Error.NotFound().ToProblem();
        }

        var totals = await query
            .Where(r => r.Type != RecordType.Transfer)
            .GroupBy(r => r.Account.Currency)
            .OrderBy(g => g.Key)
            .Select(g => new CurrencyTotals(
                g.Key,
                g.Sum(r => r.Type == RecordType.Income ? r.AccountAmount : 0m),
                g.Sum(r => r.Type == RecordType.Expense ? r.AccountAmount : 0m)))
            .ToListAsync(ct);

        return TypedResults.Ok(new RecordSummary(totals));
    }
}
