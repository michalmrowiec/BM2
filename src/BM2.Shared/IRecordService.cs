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
}