using BM2.Application.Mappings;
using BM2.Infrastructure.Repositories.Base;
using BM2.Application.Responses;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Queries.Record;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BM2.Application.Functions.Record.Queries;

public class GetAllPeriodicRecordDefinitionsQueryHandler(UnitOfWork unitOfWork)
    : IRequestHandler<GetAllPeriodicRecordDefinitionsQuery, BaseResponse<IEnumerable<PeriodicRecordDefinitionDTO>>>
{
    public async Task<BaseResponse<IEnumerable<PeriodicRecordDefinitionDTO>>> Handle(
        GetAllPeriodicRecordDefinitionsQuery request,
        CancellationToken cancellationToken)
    {
        var items = await unitOfWork.PeriodicRecordDefinitionRepository.GetAllForUserAsync(request.UserId,
            q => q.Include(x => x.RecordTemplate).ThenInclude(x => x.Wallet),
            q => q.Include(x => x.Currency),
            q => q.Include(x => x.PeriodicRecordStatus),
            q => q.Include(x => x.SetRecordStatus),
            q => q.Include(x => x.Wallet),
            q => q.Include(x => x.SetRecordAccount).ThenInclude(x => x!.DefaultCurrency));

        items.CheckPermission(request.UserId);

        return request.ReturnSuccessWithObject(items.Select(x => x.ToDto()));
    }
}
