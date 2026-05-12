using BM2.Application.Responses;

namespace BM2.Shared.Requests.Commands.Record;

public record ExecutePeriodicRecordDefinitionCommand(Guid PeriodicRecordDefinitionId) : IBaseRequest<BaseResponse>;