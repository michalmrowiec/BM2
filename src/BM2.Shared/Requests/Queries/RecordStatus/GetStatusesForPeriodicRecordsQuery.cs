using BM2.Shared.DTOs;

namespace BM2.Shared.Requests.Queries.RecordStatus;

public record GetStatusesForPeriodicRecordsQuery : IBaseRequest<IEnumerable<RecordStatusDTO>>;
