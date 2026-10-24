using BM2.Application.Mappings;
using BM2.Infrastructure.Repositories.Base;
using BM2.Application.Responses;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Queries.Record;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BM2.Application.Functions.Record.Queries;

public class GetAllRecordTemplatesQueryHandler(UnitOfWork unitOfWork)
    : IRequestHandler<GetAllRecordTemplatesQuery, BaseResponse<IEnumerable<RecordTemplateDTO>>>
{
    public async Task<BaseResponse<IEnumerable<RecordTemplateDTO>>> Handle(
        GetAllRecordTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        var data =
            await unitOfWork.RecordTemplateRepository.GetAllForUserAsync(request.UserId,
                q => q.Include(x => x.Wallet),
                q => q.Include(x => x.Currency),
                q => q.Include(x => x.Category),
                q => q.Include(x => x.Status),
                q => q.Include(x => x.Tags));

        data.CheckPermission(request.UserId);

        return request.ReturnSuccessWithObject(data.Select(x => x.ToDto()));
    }
}
