using BM2.Application.DTOs;

namespace BM2.Application.Functions.RecordStatuses.Queries.Requests;

public record GetStatusesForRecordsQuery : IBaseRequest<IEnumerable<RecordStatusDTO>>;