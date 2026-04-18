using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Responses;
using BM2.Shared.Requests.Commands.Record;
using MediatR;

namespace BM2.Application.Functions.Record.Commands;

public class DeleteRecordTemplateCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteRecordTemplateCommand, BaseResponse>
{
    public async Task<BaseResponse> Handle(DeleteRecordTemplateCommand request, CancellationToken cancellationToken)
    {
        var recordTemplate = await unitOfWork.RecordTemplateRepository.GetByIdAsync(request.Id);

        recordTemplate.ThrowExceptionIfNull();
        recordTemplate!.CheckPermission(request.OwnedByUserId);

        recordTemplate.DeletedAt = DateTime.UtcNow;
        recordTemplate.DeletedBy = request.OwnedByUserId;

        try
        {
            await unitOfWork.RecordTemplateRepository.Update(recordTemplate);
            await unitOfWork.SaveAsync();

            return request.ReturnSuccess();
        }
        catch (Exception)
        {
            return request.ReturnServerError();
        }
    }
}
