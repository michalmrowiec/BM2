using AutoMapper;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Responses;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Queries.RecordStatus;
using MediatR;

namespace BM2.Application.Functions.RecordStatus.Queries;

public class GetStatusesForPeriodicRecordsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetStatusesForPeriodicRecordsQuery, BaseResponse<IEnumerable<RecordStatusDTO>>>
{
    public async Task<BaseResponse<IEnumerable<RecordStatusDTO>>> Handle(GetStatusesForPeriodicRecordsQuery request,
        CancellationToken cancellationToken)
    {
        return request.ReturnSuccessWithObject(
            mapper.Map<IEnumerable<RecordStatusDTO>>(await unitOfWork.RecordStatusRepository.GetStatusesForPeriodicRecord()));
    }
}
