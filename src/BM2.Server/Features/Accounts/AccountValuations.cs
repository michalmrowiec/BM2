using BM2.Contracts.Accounts;
using BM2.Contracts.Records;
using BM2.Domain.Accounts;
using BM2.Domain.Common;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Accounts;

/// <summary>Manual valuations of investment accounts: read with the account, written by its editors.</summary>
internal static class AccountValuations
{
    public static void Map(RouteGroupBuilder accounts)
    {
        var valuations = accounts.MapGroup("/{accountId:guid}/valuations");
        valuations.MapGet("/", ListAsync);
        valuations.MapPut("/", SaveAsync).Validate<SaveValuationRequest>();
        valuations.MapDelete("/{valuationId:guid}", DeleteAsync);
    }

    private static async Task<Results<Ok<List<AccountValuationItem>>, ProblemHttpResult>> ListAsync(
        Guid accountId, CurrentWallet current, AppDbContext db, CancellationToken ct)
    {
        if (!current.Access.CanReadAccount(accountId) || !await IsInvestmentAsync(db, current, accountId, ct))
        {
            return Error.NotFound().ToProblem();
        }

        var valuations = await db.AccountValuations.AsNoTracking()
            .Where(v => v.AccountId == accountId)
            .OrderByDescending(v => v.ValuedOn)
            .ToListAsync(ct);

        // Paid in on each valuation day: opening balance plus completed and pending flows up to that day.
        var opening = await db.Accounts.Where(a => a.Id == accountId).Select(a => a.OpeningBalance).SingleAsync(ct);
        var flows = await db.Records.AsNoTracking()
            .Where(r => r.Status != RecordStatus.Planned && (r.AccountId == accountId || r.TransferAccountId == accountId))
            .GroupBy(r => r.OccurredOn)
            .Select(g => new
            {
                Day = g.Key,
                Net = g.Sum(r => r.AccountId == accountId
                    ? (r.Type == RecordType.Income ? r.AccountAmount : -r.AccountAmount)
                    : r.TransferAmount ?? 0),
            })
            .ToListAsync(ct);

        return TypedResults.Ok(valuations
            .Select(v => new AccountValuationItem(v.Id, v.ValuedOn, v.Value, opening + flows.Where(f => f.Day <= v.ValuedOn).Sum(f => f.Net)))
            .ToList());
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> SaveAsync(
        Guid accountId, SaveValuationRequest request, CurrentWallet current, AppDbContext db, TimeProvider time, CancellationToken ct)
    {
        if (!current.Access.CanReadAccount(accountId) || !await IsInvestmentAsync(db, current, accountId, ct))
        {
            return Error.NotFound().ToProblem();
        }

        if (!current.Access.CanWriteAccount(accountId))
        {
            return Error.Forbidden().ToProblem();
        }

        var existing = await db.AccountValuations.SingleOrDefaultAsync(v => v.AccountId == accountId && v.ValuedOn == request.ValuedOn, ct);
        if (existing is null)
        {
            db.AccountValuations.Add(AccountValuation.Create(accountId, request.ValuedOn, request.Value, time.GetUtcNow()));
        }
        else
        {
            existing.Change(request.Value, time.GetUtcNow());
        }

        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteAsync(
        Guid accountId, Guid valuationId, CurrentWallet current, AppDbContext db, CancellationToken ct)
    {
        if (!current.Access.CanReadAccount(accountId) || !await IsInvestmentAsync(db, current, accountId, ct))
        {
            return Error.NotFound().ToProblem();
        }

        if (!current.Access.CanWriteAccount(accountId))
        {
            return Error.Forbidden().ToProblem();
        }

        var deleted = await db.AccountValuations.Where(v => v.Id == valuationId && v.AccountId == accountId).ExecuteDeleteAsync(ct);
        return deleted == 0 ? Error.NotFound().ToProblem() : TypedResults.NoContent();
    }

    private static Task<bool> IsInvestmentAsync(AppDbContext db, CurrentWallet current, Guid accountId, CancellationToken ct) =>
        db.Accounts.AnyAsync(a => a.Id == accountId && a.WalletId == current.Id && a.Kind == AccountKind.Investment, ct);
}
