using BM2.Application.Mappings;
using BM2.Infrastructure.Repositories.Base;
using BM2.Application.Functions.Record.Commands.Validators;
using BM2.Application.Responses;
using BM2.Application.Services;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.Record;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BM2.Application.Functions.Record.Commands;

public class UpdatePeriodicRecordDefinitionCommandHandler(
    UnitOfWork _unitOfWork, IBackgroundJobClient _backgroundJobClient, IPeriodicJobManager _periodicJobManager, IMediator mediator)
    : IRequestHandler<UpdatePeriodicRecordDefinitionCommand, BaseResponse<PeriodicRecordDefinitionDTO>>
{
    public async Task<BaseResponse<PeriodicRecordDefinitionDTO>> Handle(UpdatePeriodicRecordDefinitionCommand request,
        CancellationToken cancellationToken)
    {
        var validationResult =
            await new AddPeriodicRecordDefinitionCommandValidator(_unitOfWork, false).ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid) return new BaseResponse<PeriodicRecordDefinitionDTO>(validationResult);

        var entity = await _unitOfWork.PeriodicRecordDefinitionRepository.GetByIdAsync(request.Id);
        entity.ThrowExceptionIfNull();
        entity!.CheckPermission(request.OwnedByUserId);

        entity.Apply(request);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = request.OwnedByUserId;

        try
        {
            _periodicJobManager.RemoveScheduledJob(entity);
            entity.NextExecutionAt = entity.StartDate;

            await _unitOfWork.PeriodicRecordDefinitionRepository.Update(entity);
            await _unitOfWork.SaveAsync();

            await _periodicJobManager.ScheduleOrExecuteOfFirstCycle(entity, mediator);

            var updated = await _unitOfWork.PeriodicRecordDefinitionRepository.GetByIdAsync(entity.Id,
                q => q.Include(x => x.RecordTemplate).ThenInclude(x => x.Wallet),
                q => q.Include(x => x.Currency),
                q => q.Include(x => x.PeriodicRecordStatus),
                q => q.Include(x => x.SetRecordStatus),
                q => q.Include(x => x.Wallet),
                q => q.Include(x => x.SetRecordAccount).ThenInclude(x => x!.DefaultCurrency));

            updated.ThrowExceptionIfNull();

            return request.ReturnSuccessWithObject(updated.ToDto());
        }
        catch (Exception)
        {
            return request.ReturnServerError();
        }
    }
}
