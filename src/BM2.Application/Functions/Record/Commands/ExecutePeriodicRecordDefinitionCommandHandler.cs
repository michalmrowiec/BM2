using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Responses;
using BM2.Application.Services;
using BM2.Shared.Requests.Commands.Record;
using BM2.Shared.SystemCodes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BM2.Application.Functions.Record.Commands;

public class ExecutePeriodicRecordDefinitionCommandHandler(
    IUnitOfWork uow,
    IMediator mediator,
    IPeriodicJobManager _periodicJobManager)
    : IRequestHandler<ExecutePeriodicRecordDefinitionCommand, BaseResponse>
{
    public async Task<BaseResponse> Handle(ExecutePeriodicRecordDefinitionCommand request, CancellationToken ct)
    {
        var definition = await uow.PeriodicRecordDefinitionRepository.GetByIdAsync(request.PeriodicRecordDefinitionId,
                q => q.Include(x => x.RecordTemplate).ThenInclude(x => x.Wallet),
                q => q.Include(x => x.Currency),
                q => q.Include(x => x.PeriodicRecordStatus),
                q => q.Include(x => x.SetRecordStatus),
                q => q.Include(x => x.Wallet),
                q => q.Include(x => x.SetRecordAccount).ThenInclude(x => x!.DefaultCurrency));

        definition.ThrowExceptionIfNull();

        if (definition!.PeriodicRecordStatus?.SystemCode != StatusSystemCode.Active)
            return new BaseResponse(BaseResponse.ResponseStatus.Success, "Definition not found or not active.");

        var recordTemplate = definition.RecordTemplate;
        recordTemplate.ThrowExceptionIfNull();

        var newRecordCommand = new AddRecordCommand()
        {
            CategoryId = recordTemplate!.CategoryId,
            StatusId = recordTemplate!.StatusId,
            Name = recordTemplate!.Name,
            Description = recordTemplate!.Description,
            Amount = recordTemplate!.Amount,
            PlannedAmount = recordTemplate!.PlannedAmount,
            CurrencyId = definition.CurrencyId,
            TagIds = recordTemplate!.Tags.Select(t => t.Id).ToList(),
            OwnedByUserId = recordTemplate!.OwnedByUserId,
            AccountId = definition.SetRecordAccountId,
            RecordDateTime = DateTime.UtcNow,
        };

        await mediator.Send(newRecordCommand, ct);

        _periodicJobManager.ScheduleNextExecution(definition);

        await uow.SaveAsync();

        return new BaseResponse();
    }
}