using BM2.Application.Mappings;
using BM2.Infrastructure.Repositories.Base;
using BM2.Application.Functions.Record.Commands.Validators;
using BM2.Application.Responses;
using BM2.Domain.Entities.UserRecords;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.Record;
using MediatR;

namespace BM2.Application.Functions.Record.Commands;

public class UpdateRecordCommandHandler(UnitOfWork unitOfWork)
    : IRequestHandler<UpdateRecordCommand, BaseResponse<RecordDTO>>
{
    public async Task<BaseResponse<RecordDTO>> Handle(UpdateRecordCommand request, CancellationToken cancellationToken)
    {
        var validationResult =
            await new AddRecordCommandValidator(unitOfWork, false).ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid) return new BaseResponse<RecordDTO>(validationResult);


        var record = (await unitOfWork.RecordRepository.GetByIdAsync(request.Id)).EnsureFound();

        record!.CheckPermission(request.OwnedByUserId);

        var recordTagRelations =
            await unitOfWork.RecordTagRelationRepository.GetListByAsync(x => x.RecordId == request.Id);

        recordTagRelations.ThrowExceptionIfNull();
        recordTagRelations!.CheckPermission(request.OwnedByUserId);

        var toAdd = request.TagIds
            .Where(x => recordTagRelations.All(y => y.TagId != x))
            .Select(tagId => RecordTagRelation.CreateInstance(record!.Id, tagId, request.OwnedByUserId))
            .ToList();

        var toDelete = recordTagRelations
            .Where(x => !request.TagIds.Contains(x.TagId))
            .ToList();

        record.Apply(request);

        record!.UpdatedAt = DateTime.UtcNow;
        record.UpdatedBy = request.OwnedByUserId;

        try
        {
            record = await unitOfWork.RecordRepository.Update(record);
            await unitOfWork.RecordTagRelationRepository.AddRange(toAdd);
            await unitOfWork.RecordTagRelationRepository.Delete(toDelete);
            await unitOfWork.SaveAsync();

            return request.ReturnSuccessWithObject(record.ToDto());
        }
        catch (Exception e)
        {
            return request.ReturnServerError();
        }
    }
}