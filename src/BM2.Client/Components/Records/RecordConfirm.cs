using BM2.Client.Services;
using BM2.Contracts.Records;

namespace BM2.Client.Components.Records;

/// <summary>Changes the status of pending or planned records (completed by default), keeping everything else as it is.</summary>
public static class RecordConfirm
{
    /// <returns>The number of records that failed.</returns>
    public static async Task<int> ConfirmAsync(
        ApiClient api, Guid walletId, IEnumerable<RecordItem> records, RecordStatus status = RecordStatus.Completed)
    {
        var failed = 0;
        foreach (var r in records)
        {
            var request = new SaveRecordRequest(
                r.Type, status, r.OccurredOn, r.AccountId, r.Amount, r.Currency, r.AccountAmount, r.CategoryId,
                r.TransferAccountId, r.TransferAmount, r.Name, r.TagIds);
            var result = await api.PutAsync($"api/wallets/{walletId}/records/{r.Id}", request);
            failed += result.Ok ? 0 : 1;
        }

        return failed;
    }
}
