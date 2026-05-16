using BM2.Application.Mappings;
using BM2.Infrastructure.Repositories.Base;
using BM2.Application.Functions.Record.Commands.Validators;
using BM2.Application.Responses;
using BM2.Application.Services;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.Record;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BM2.Application.Functions.Record.Commands;

public class AddPeriodicRecordDefinitionCommandHandler(UnitOfWork unitOfWork, IPeriodicJobManager _periodicJobManager)
    : IRequestHandler<AddPeriodicRecordDefinitionCommand, BaseResponse<PeriodicRecordDefinitionDTO>>
{
    public async Task<BaseResponse<PeriodicRecordDefinitionDTO>> Handle(
        AddPeriodicRecordDefinitionCommand request,
        CancellationToken cancellationToken)
    {
        var validationResult =
            await new AddPeriodicRecordDefinitionCommandValidator(unitOfWork).ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid) return new BaseResponse<PeriodicRecordDefinitionDTO>(validationResult);

        var entity = request.ToEntity();
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = request.OwnedByUserId;

        try
        {
            await unitOfWork.PeriodicRecordDefinitionRepository.Add(entity);
            _periodicJobManager.ScheduleNextExecution(entity);
            await unitOfWork.SaveAsync();

            // zaplanuj kolejny rekord, czyli wywołaj serwis który wyznaczy next date oraz zarejestruje zadanie w hangfire
            // hangfire wykona zodanie które -
            // 1. utworzy rekord na podstawie tej definicji
            // 2. zaktualizuje next date w definicji i ponownie zarejestruje zadanie w hangfire
            // czyli potrzebujemy jeszcze command który de facto będzie "ExecutePeriodicRecordDefinitionCommand" i handler który wykona powyższe kroki

            var created = await unitOfWork.PeriodicRecordDefinitionRepository.GetByIdAsync(entity.Id,
                q => q.Include(x => x.RecordTemplate).ThenInclude(x => x.Wallet),
                q => q.Include(x => x.Currency),
                q => q.Include(x => x.PeriodicRecordStatus),
                q => q.Include(x => x.SetRecordStatus),
                q => q.Include(x => x.Wallet),
                q => q.Include(x => x.SetRecordAccount).ThenInclude(x => x!.DefaultCurrency));

            created.ThrowExceptionIfNull();

            return request.ReturnSuccessWithObject(created.ToDto());
        }
        catch (Exception)
        {
            return request.ReturnServerError();
        }
    }
}
