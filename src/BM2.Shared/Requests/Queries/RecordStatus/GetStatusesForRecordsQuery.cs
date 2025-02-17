using BM2.Shared.DTOs;

namespace BM2.Shared.Requests.Queries.RecordStatus;

public record GetStatusesForRecordsQuery : IBaseRequest<IEnumerable<RecordStatusDTO>>;