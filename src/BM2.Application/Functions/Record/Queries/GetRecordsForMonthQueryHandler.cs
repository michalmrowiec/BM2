using AutoMapper;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Responses;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Queries.Record;
using MediatR;

namespace BM2.Application.Functions.Record.Queries;

public class GetRecordsForMonthQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
    : IRequestHandler<GetRecordsForMonthQuery, BaseResponse<IEnumerable<RecordDTO>>>
{
    public async Task<BaseResponse<IEnumerable<RecordDTO>>> Handle(
        GetRecordsForMonthQuery request,
        CancellationToken cancellationToken)
    {
        var records =
            await unitOfWork.RecordRepository.GetAllForMonthAsync(request.UserId, request.Year, request.Month);

        records.CheckPermission(request.UserId);

        return request.ReturnSuccessWithObject(mapper.Map<IEnumerable<RecordDTO>>(records));
    }
}