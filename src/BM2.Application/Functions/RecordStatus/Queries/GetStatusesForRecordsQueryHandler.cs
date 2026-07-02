using BM2.Application.Mappings;
using BM2.Infrastructure.Repositories.Base;
using BM2.Application.Responses;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Queries.RecordStatus;
using MediatR;

namespace BM2.Application.Functions.RecordStatus.Queries;

public class GetStatusesForRecordsQueryHandler(UnitOfWork unitOfWork)
    : IRequestHandler<GetStatusesForRecordsQuery, BaseResponse<IEnumerable<RecordStatusDTO>>>
{
    public async Task<BaseResponse<IEnumerable<RecordStatusDTO>>> Handle(GetStatusesForRecordsQuery request,
        CancellationToken cancellationToken)
    {
        return request.ReturnSuccessWithObject(
            (await unitOfWork.RecordStatusRepository.GetStatusesForRecords()).Select(x => x.ToDto()));
    }
}