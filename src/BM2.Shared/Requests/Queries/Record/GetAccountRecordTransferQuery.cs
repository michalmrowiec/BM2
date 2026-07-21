using BM2.Shared.DTOs;

namespace BM2.Shared.Requests.Queries.Record;

public record GetAccountRecordTransferQuery(Guid UserId, Guid Id) : IBaseRequest<AccountRecordTransferDTO>;