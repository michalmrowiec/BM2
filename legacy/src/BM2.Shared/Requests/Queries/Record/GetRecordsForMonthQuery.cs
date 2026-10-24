using BM2.Shared.DTOs;

namespace BM2.Shared.Requests.Queries.Record;

public record GetRecordsForMonthQuery(Guid UserId, int Year, int Month, Guid? WalletId = null) : IBaseRequestCollection<RecordDTO>;