using BM2.Application.Mappings;
using BM2.Infrastructure.Repositories.Base;
using BM2.Application.Functions.Record.Commands.Validators;
using BM2.Application.Responses;
using BM2.Domain.Entities.UserRecords;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.Record;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BM2.Application.Functions.Record.Commands;

public class UpdateRecordTemplateCommandHandler(UnitOfWork unitOfWork)
    : IRequestHandler<UpdateRecordTemplateCommand, BaseResponse<RecordTemplateDTO>>
{
    public async Task<BaseResponse<RecordTemplateDTO>> Handle(UpdateRecordTemplateCommand request, CancellationToken cancellationToken)
    {
        var validationResult =
            await new AddRecordTemplateCommandValidator(unitOfWork, false).ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid) return new BaseResponse<RecordTemplateDTO>(validationResult);

        var recordTemplate = await unitOfWork.RecordTemplateRepository.GetByIdAsync(request.Id);

        recordTemplate.ThrowExceptionIfNull();
        recordTemplate!.CheckPermission(request.OwnedByUserId);

        var recordTagRelations =
            await unitOfWork.RecordTagRelationRepository.GetListByAsync(x => x.RecordId == request.Id);

        recordTagRelations.ThrowExceptionIfNull();
        recordTagRelations!.CheckPermission(request.OwnedByUserId);

        var toAdd = request.TagIds
            .Where(x => recordTagRelations.All(y => y.TagId != x))
            .Select(tagId => RecordTagRelation.CreateInstance(recordTemplate.Id, tagId, request.OwnedByUserId))
            .ToList();

        var toDelete = recordTagRelations
            .Where(x => !request.TagIds.Contains(x.TagId))
            .ToList();

        recordTemplate.Apply(request);

        recordTemplate.UpdatedAt = DateTime.UtcNow;
        recordTemplate.UpdatedBy = request.OwnedByUserId;

        try
        {
            await unitOfWork.RecordTemplateRepository.Update(recordTemplate);
            await unitOfWork.RecordTagRelationRepository.AddRange(toAdd);
            await unitOfWork.RecordTagRelationRepository.Delete(toDelete);
            await unitOfWork.SaveAsync();

            var updatedRecordTemplate = await unitOfWork.RecordTemplateRepository.GetByIdAsync(request.Id,
                q => q.Include(x => x.Wallet),
                q => q.Include(x => x.Currency),
                q => q.Include(x => x.Category),
                q => q.Include(x => x.Status),
                q => q.Include(x => x.Tags));

            updatedRecordTemplate.ThrowExceptionIfNull();

            return request.ReturnSuccessWithObject(updatedRecordTemplate.ToDto());
        }
        catch (Exception)
        {
            return request.ReturnServerError();
        }
    }
}
