using BM2.Contracts.Records;
using BM2.Contracts.Recurring;
using BM2.Domain.Access;
using BM2.Domain.Common;
using BM2.Domain.Records;
using BM2.Domain.Recurring;
using BM2.Server.Data;
using BM2.Server.Features.Records;
using BM2.Server.Features.Wallets;
using BM2.Server.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Recurring;

public static class RecurringEndpoints
{
    public static RouteGroupBuilder MapRecurringEndpoints(this RouteGroupBuilder wallet)
    {
        var rules = wallet.MapGroup("/recurring");
        rules.MapGet("/", ListAsync);
        rules.MapPost("/", CreateAsync).Validate<SaveRecurringRuleRequest>();
        rules.MapPut("/{ruleId:guid}", UpdateAsync).Validate<SaveRecurringRuleRequest>();
        rules.MapDelete("/{ruleId:guid}", DeleteAsync);
        return wallet;
    }

    private static async Task<Ok<List<RecurringRuleItem>>> ListAsync(CurrentWallet current, AppDbContext db, CancellationToken ct)
    {
        var access = current.Access;
        var walletId = current.Id;
        var rules = await db.RecurringRules
            .AsNoTracking()
            .Include(r => r.Tags)
            .Where(r => r.WalletId == walletId)
            .OrderBy(r => r.NextDate == null).ThenBy(r => r.NextDate)
            .ToListAsync(ct);

        var counts = await db.Records
            .Where(r => r.RecurringRuleId != null && r.Account.WalletId == walletId)
            .GroupBy(r => r.RecurringRuleId!.Value)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Key, g => g.Count, ct);

        return TypedResults.Ok(rules
            .Where(r => access.CanReadRecord(r.AccountId, r.TransferAccountId))
            .Select(r => ToItem(r, counts.GetValueOrDefault(r.Id)))
            .ToList());
    }

    private static async Task<Results<Created<RecurringRuleItem>, ProblemHttpResult>> CreateAsync(
        SaveRecurringRuleRequest request, CurrentWallet current, ICurrentUser currentUser, AppDbContext db, TimeProvider time,
        CancellationToken ct)
    {
        if (await CheckAsync(db, current.Access, request, currentUser.UserId, time, ct) is { } error)
        {
            return error.ToProblem();
        }

        Record? first = null;
        if (request.FromRecordId is { } recordId)
        {
            first = await db.Records.SingleOrDefaultAsync(r => r.Id == recordId && r.Account.WalletId == current.Id, ct);
            if (first is null || !current.Access.CanWriteRecord(first.AccountId, first.TransferAccountId))
            {
                return Error.NotFound().ToProblem();
            }
        }

        var now = time.GetUtcNow();
        var rule = RecurringRule.Create(current.Id, request, currentUser.UserId, now, first?.OccurredOn);
        db.RecurringRules.Add(rule);
        if (first is not null && first.RecurringRuleId is null)
        {
            first.LinkToRule(rule.Id, first.OccurredOn);
        }

        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"{ApiPaths.Prefix}/wallets/{current.Id}/recurring/{rule.Id}", ToItem(rule, first is null ? 0 : 1));
    }

    /// <param name="applyToPast">Also rewrites the fields of records the rule already added (dates and statuses stay).</param>
    private static async Task<Results<NoContent, ProblemHttpResult>> UpdateAsync(
        Guid ruleId, SaveRecurringRuleRequest request, CurrentWallet current, ICurrentUser currentUser, AppDbContext db,
        TimeProvider time, CancellationToken ct, bool applyToPast = false)
    {
        var rule = await db.RecurringRules.Include(r => r.Tags).SingleOrDefaultAsync(r => r.Id == ruleId && r.WalletId == current.Id, ct);
        if (rule is null || !current.Access.CanReadRecord(rule.AccountId, rule.TransferAccountId))
        {
            return Error.NotFound().ToProblem();
        }

        if (!current.Access.CanWriteRecord(rule.AccountId, rule.TransferAccountId))
        {
            return Error.Forbidden().ToProblem();
        }

        if (await CheckAsync(db, current.Access, request, currentUser.UserId, time, ct) is { } error)
        {
            return error.ToProblem();
        }

        rule.Update(request, Today(time));
        if (applyToPast && await ApplyToPastAsync(db, current.Id, rule, time.GetUtcNow(), ct) is { } pastError)
        {
            return pastError.ToProblem();
        }

        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    // Added records stay unless withRecords; the foreign key sets their rule id to null.
    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteAsync(
        Guid ruleId, CurrentWallet current, AppDbContext db, CancellationToken ct, bool withRecords = false)
    {
        var rule = await db.RecurringRules
            .AsNoTracking()
            .Where(r => r.Id == ruleId && r.WalletId == current.Id)
            .Select(r => new { r.AccountId, r.TransferAccountId })
            .SingleOrDefaultAsync(ct);
        if (rule is null || !current.Access.CanReadRecord(rule.AccountId, rule.TransferAccountId))
        {
            return Error.NotFound().ToProblem();
        }

        if (!current.Access.CanWriteRecord(rule.AccountId, rule.TransferAccountId))
        {
            return Error.Forbidden().ToProblem();
        }

        if (withRecords)
        {
            var accounts = await db.Records.Where(r => r.RecurringRuleId == ruleId)
                .Select(r => new { r.AccountId, r.TransferAccountId }).ToListAsync(ct);
            if (!accounts.TrueForAll(a => current.Access.CanWriteRecord(a.AccountId, a.TransferAccountId)))
            {
                return Error.Forbidden().ToProblem();
            }

            await db.Records.Where(r => r.RecurringRuleId == ruleId).ExecuteDeleteAsync(ct);
        }

        await db.RecurringRules.Where(r => r.Id == ruleId).ExecuteDeleteAsync(ct);
        return TypedResults.NoContent();
    }

    /// <summary>The same checks as a new record dated on the schedule start.</summary>
    private static async Task<Error?> CheckAsync(
        AppDbContext db, WalletAccess access, SaveRecurringRuleRequest request, Guid userId, TimeProvider time, CancellationToken ct)
    {
        var draft = (request.Record with { OccurredOn = request.Schedule.StartDate, Status = RecordStatus.Completed }).ToDraft();
        var context = await RecordContextLoader.LoadAsync(db, access.WalletId, draft, ct);
        if (RecordAccess.CheckWrite(access, draft, context) is { } denied)
        {
            return denied;
        }

        var record = Record.Create(draft, context, userId, time.GetUtcNow());
        return record.IsSuccess ? null : record.Error;
    }

    // Installment numbers follow the order of the added records.
    private static async Task<Error?> ApplyToPastAsync(AppDbContext db, Guid walletId, RecurringRule rule, DateTimeOffset now, CancellationToken ct)
    {
        var records = await db.Records
            .Include(r => r.Tags)
            .Where(r => r.RecurringRuleId == rule.Id)
            .OrderBy(r => r.OccurrenceDate).ThenBy(r => r.OccurredOn)
            .ToListAsync(ct);
        if (records.Count == 0)
        {
            return null;
        }

        var context = await RecordContextLoader.LoadAsync(db, walletId, rule.DraftFor(0, records[0].OccurredOn, records[0].Status), ct);
        for (var i = 0; i < records.Count; i++)
        {
            var record = records[i];
            if (record.Update(rule.DraftFor(i, record.OccurredOn, record.Status), context, now).Error is { } error)
            {
                return error;
            }
        }

        return null;
    }

    internal static DateOnly Today(TimeProvider time) => DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime);

    private static RecurringRuleItem ToItem(RecurringRule r, int recordCount) => new(
        r.Id,
        new SaveRecordRequest(
            r.Type, RecordStatus.Completed, r.StartDate, r.AccountId, r.Amount, r.Currency, r.AccountAmount, r.CategoryId,
            r.TransferAccountId, r.TransferAmount, r.Name, r.Tags.Select(t => t.TagId).ToList()),
        r.Schedule,
        r.Mode,
        r.IsPaused,
        r.NextIndex,
        r.NextDate,
        recordCount);
}

internal static partial class RecurringCatchUp
{
    /// <summary>
    /// Adds every due occurrence up to today. Idempotent: the unique (rule, occurrence date) index rejects a second
    /// copy, so a parallel run that loses simply stops. Occurrences whose account or category is archived or gone are
    /// skipped.
    /// </summary>
    public static async Task RunAsync(AppDbContext db, Guid walletId, TimeProvider time, ILogger logger, CancellationToken ct)
    {
        var today = RecurringEndpoints.Today(time);
        var rules = await db.RecurringRules
            .Include(r => r.Tags)
            .Where(r => r.WalletId == walletId && !r.IsPaused && r.NextDate != null && r.NextDate <= today)
            .ToListAsync(ct);
        if (rules.Count == 0)
        {
            return;
        }

        var now = time.GetUtcNow();
        foreach (var rule in rules)
        {
            while (rule.NextDate is { } date && date <= today)
            {
                var draft = rule.NextDraft();
                var context = await RecordContextLoader.LoadAsync(db, walletId, draft, ct);
                var created = Record.Create(draft, context, rule.CreatedByUserId, now);
                if (created.IsSuccess)
                {
                    created.Value.LinkToRule(rule.Id, date);
                    db.Records.Add(created.Value);
                }

                rule.Advance();
            }
        }

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            LogLostRace(logger, ex, walletId);
            db.ChangeTracker.Clear();
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Recurring catch-up for wallet {WalletId} lost to a parallel run.")]
    private static partial void LogLostRace(ILogger logger, Exception exception, Guid walletId);
}
