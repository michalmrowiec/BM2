using BM2.Application.DTOs;

namespace BM2.Application.Functions.RecordStatus.Queries.Requests;

public record GetStatusesForRecordsQuery : IBaseRequest<IEnumerable<RecordStatusDTO>>;