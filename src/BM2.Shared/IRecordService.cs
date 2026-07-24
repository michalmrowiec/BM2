using BM2.Shared.DTOs;

namespace BM2.Shared;

public interface IRecordService
{
    public Task<(List<RecordDTO> Items, int TotalCount)> GetPagedRecordsAsync(
        int page,
        int pageSize,
        string? sortBy,
        bool sortDescending,
        Guid walletId,
        TransactionFilter filter);

    public Task<decimal> GetSum(Guid walletId, TransactionFilter filter);
    public Task<List<RecordSum>> GetSumForAccounts(Guid walletId, TransactionFilter filter);
}

public record RecordSum(AccountDTO Account, decimal Amount, CurrencyDTO Currency);