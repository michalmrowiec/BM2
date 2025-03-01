using AutoMapper;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Functions.Record.Commands.Validators;
using BM2.Application.Responses;
using BM2.Domain.Entities.UserRecords;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.Record;
using MediatR;

namespace BM2.Application.Functions.Record.Commands;

public class UpdateRecordCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateRecordCommand, BaseResponse<RecordDTO>>
{
    public async Task<BaseResponse<RecordDTO>> Handle(UpdateRecordCommand request, CancellationToken cancellationToken)
    {
        var validationResult =
            await new AddRecordCommandValidator(unitOfWork, false).ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid) return new BaseResponse<RecordDTO>(validationResult);


        var record = await unitOfWork.RecordRepository.GetByIdAsync(request.Id);

        record.ThrowExceptionIfNull();
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

        mapper.Map(request, record);

        record!.UpdatedAt = DateTime.UtcNow;
        record.UpdatedBy = request.OwnedByUserId;

        try
        {
            record = await unitOfWork.RecordRepository.Update(record);
            await unitOfWork.RecordTagRelationRepository.AddRange(toAdd);
            await unitOfWork.RecordTagRelationRepository.Delete(toDelete);
            await unitOfWork.SaveAsync();

            return request.ReturnSuccessWithObject(mapper.Map<Domain.Entities.UserRecords.Record, RecordDTO>(record));
        }
        catch (Exception e)
        {
            return request.ReturnServerError();
        }
    }
}