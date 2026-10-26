using System.Linq.Expressions;
using BM2.Contracts.Categories;
using BM2.Contracts.Records;
using BM2.Domain.Records;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Records;

internal static class RecordQueries
{
    public static readonly Expression<Func<Record, RecordItem>> ToItem = r => new RecordItem(
        r.Id,
        r.Type,
        r.Status,
        r.OccurredOn,
        r.AccountId,
        r.Account.Name,
        r.Amount,
        r.Currency,
        r.AccountAmount,
        r.CategoryId,
        r.TransferAccountId,
        r.TransferAccount != null ? r.TransferAccount.Name : null,
        r.TransferAmount,
        r.Name,
        r.Tags.Select(t => t.TagId).ToList(),
        r.RecurringRuleId);

    /// <summary>Records touching an account the user can read (either side of a transfer).</summary>
    public static IQueryable<Record> Visible(this IQueryable<Record> records, IReadOnlyCollection<Guid> readableAccountIds) =>
        records.Where(r => readableAccountIds.Contains(r.AccountId)
            || (r.TransferAccountId != null && readableAccountIds.Contains(r.TransferAccountId.Value)));

    /// <summary>Visible records matching the filter; null when the filter names an account the user cannot read.</summary>
    public static IQueryable<Record>? Filtered(AppDbContext db, CurrentWallet current, RecordFilter filter)
    {
        var access = current.Access;
        if (filter.AccountId is { } accountId && !access.CanReadAccount(accountId))
        {
            return null;
        }

        var query = db.Records.AsNoTracking().Visible(access.ReadableAccountIds);
        if (filter.From is { } from)
        {
            query = query.Where(r => r.OccurredOn >= from);
        }

        if (filter.To is { } to)
        {
            query = query.Where(r => r.OccurredOn < to);
        }

        if (filter.AccountId is { } account)
        {
            query = query.Where(r => r.AccountId == account || r.TransferAccountId == account);
        }

        if (filter.CategoryId is { } category)
        {
            query = query.Where(r => r.CategoryId == category);
        }

        if (filter.TagId is { } tag)
        {
            query = query.Where(r => r.Tags.Any(t => t.TagId == tag));
        }

        // Refunds (income in an expense category) match expenses, like in stats.
        if (filter.Type is { } type)
        {
            query = type switch
            {
                RecordType.Expense => query.Where(r => r.Type == RecordType.Expense
                    || (r.Type == RecordType.Income && r.Category!.Type == CategoryType.Expense)),
                RecordType.Income => query.Where(r => r.Type == RecordType.Income && r.Category!.Type != CategoryType.Expense),
                _ => query.Where(r => r.Type == type),
            };
        }

        if (filter.Status is { } status)
        {
            query = query.Where(r => r.Status == status);
        }

        if (filter.RuleId is { } ruleId)
        {
            query = query.Where(r => r.RecurringRuleId == ruleId);
        }
        else if (filter.Recurring == true)
        {
            query = query.Where(r => r.RecurringRuleId != null);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            // Unnamed records are shown by their category, so the category name is searched too.
            var term = filter.Search.Trim();
            query = query.Where(r => (r.Name != null && r.Name.Contains(term))
                || (r.Name == null && r.Category != null && r.Category.Name.Contains(term)));
        }

        return query;
    }

    public static RecordDraft ToDraft(this SaveRecordRequest request) => new(
        request.Type,
        request.Status,
        request.OccurredOn,
        request.AccountId,
        request.Amount,
        request.Currency,
        request.AccountAmount,
        request.CategoryId,
        request.TransferAccountId,
        request.TransferAmount,
        request.Name,
        request.TagIds);
}
