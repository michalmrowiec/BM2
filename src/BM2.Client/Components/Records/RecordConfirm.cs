using BM2.Client.Services;
using BM2.Contracts.Records;

namespace BM2.Client.Components.Records;

/// <summary>Changes the status of pending or planned records (completed by default), keeping everything else as it is.</summary>
public static class RecordConfirm
{
    /// <param name="amount">The actual amount, when it differs; amounts in other currencies scale by the record's rate.</param>
    /// <returns>The number of records that failed.</returns>
    public static async Task<int> ConfirmAsync(
        ApiClient api, Guid walletId, IEnumerable<RecordItem> records, RecordStatus status = RecordStatus.Completed,
        decimal? amount = null)
    {
        var failed = 0;
        foreach (var r in records)
        {
            var newAmount = amount ?? r.Amount;
            var request = new SaveRecordRequest(
                r.Type, status, r.OccurredOn, r.AccountId, newAmount, r.Currency, Scale(r, r.AccountAmount, newAmount) ?? newAmount,
                r.CategoryId, r.TransferAccountId, Scale(r, r.TransferAmount, newAmount), r.Name, r.TagIds);
            var result = await api.PutAsync($"api/wallets/{walletId}/records/{r.Id}", request);
            failed += result.Ok ? 0 : 1;
        }

        return failed;
    }

    // An amount tied to the record amount (account or received currency), moved along with it.
    private static decimal? Scale(RecordItem r, decimal? other, decimal newAmount) => other switch
    {
        null => null,
        { } o when newAmount == r.Amount => o,
        { } o when o == r.Amount => newAmount,
        { } o when r.Amount > 0 => ExchangeMath.ToAccountAmount(newAmount, ExchangeMath.RateOf(r.Amount, o)),
        { } o => o,
    };
}
