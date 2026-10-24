using System.Linq.Expressions;
using BM2.Contracts.Records;
using BM2.Domain.Records;

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
        r.Tags.Select(t => t.TagId).ToList());

    /// <summary>Records touching an account the user can read (either side of a transfer).</summary>
    public static IQueryable<Record> Visible(this IQueryable<Record> records, IReadOnlyCollection<Guid> readableAccountIds) =>
        records.Where(r => readableAccountIds.Contains(r.AccountId)
            || (r.TransferAccountId != null && readableAccountIds.Contains(r.TransferAccountId.Value)));

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
