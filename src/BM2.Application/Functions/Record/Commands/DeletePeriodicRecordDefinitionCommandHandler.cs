using BM2.Infrastructure.Repositories.Base;
using BM2.Application.Responses;
using BM2.Shared.Requests.Commands.Record;
using MediatR;

namespace BM2.Application.Functions.Record.Commands;

public class DeletePeriodicRecordDefinitionCommandHandler(UnitOfWork unitOfWork)
    : IRequestHandler<DeletePeriodicRecordDefinitionCommand, BaseResponse>
{
    public async Task<BaseResponse> Handle(DeletePeriodicRecordDefinitionCommand request, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.PeriodicRecordDefinitionRepository.GetByIdAsync(request.Id);
        entity.ThrowExceptionIfNull();
        entity!.CheckPermission(request.OwnedByUserId);

        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = request.OwnedByUserId;

        try
        {
            await unitOfWork.PeriodicRecordDefinitionRepository.Update(entity);
            await unitOfWork.SaveAsync();

            return request.ReturnSuccess();
        }
        catch (Exception)
        {
            return request.ReturnServerError();
        }
    }
}
