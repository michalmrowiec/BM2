using AutoMapper;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Functions.Record.Commands.Validators;
using BM2.Application.Responses;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.Record;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BM2.Application.Functions.Record.Commands;

public class AddPeriodicRecordDefinitionCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
    : IRequestHandler<AddPeriodicRecordDefinitionCommand, BaseResponse<PeriodicRecordDefinitionDTO>>
{
    public async Task<BaseResponse<PeriodicRecordDefinitionDTO>> Handle(AddPeriodicRecordDefinitionCommand request,
        CancellationToken cancellationToken)
    {
        var validationResult =
            await new AddPeriodicRecordDefinitionCommandValidator(unitOfWork).ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid) return new BaseResponse<PeriodicRecordDefinitionDTO>(validationResult);

        var entity = mapper.Map<Domain.Entities.UserRecords.PeriodicRecordDefinition>(request);
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = request.OwnedByUserId;

        try
        {
            await unitOfWork.PeriodicRecordDefinitionRepository.Add(entity);
            await unitOfWork.SaveAsync();

            var created = await unitOfWork.PeriodicRecordDefinitionRepository.GetByIdAsync(entity.Id,
                q => q.Include(x => x.RecordTemplate).ThenInclude(x => x.Wallet),
                q => q.Include(x => x.Currency),
                q => q.Include(x => x.PeriodicRecordStatus),
                q => q.Include(x => x.SetRecordStatus),
                q => q.Include(x => x.Wallet),
                q => q.Include(x => x.SetRecordAccount).ThenInclude(x => x!.DefaultCurrency));

            created.ThrowExceptionIfNull();

            return request.ReturnSuccessWithObject(mapper.Map<PeriodicRecordDefinitionDTO>(created));
        }
        catch (Exception)
        {
            return request.ReturnServerError();
        }
    }
}
