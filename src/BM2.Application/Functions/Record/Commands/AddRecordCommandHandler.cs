using AutoMapper;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Functions.Record.Commands.Validators;
using BM2.Application.Responses;
using BM2.Domain.Entities.UserRecords;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.Record;
using MediatR;

namespace BM2.Application.Functions.Record.Commands;

public class AddRecordCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
    : IRequestHandler<AddRecordCommand, BaseResponse<RecordDTO>>
{
    public async Task<BaseResponse<RecordDTO>> Handle(AddRecordCommand request, CancellationToken cancellationToken)
    {
        var validationResult =
            await new AddRecordCommandValidator(unitOfWork).ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid) return new BaseResponse<RecordDTO>(validationResult);

        var record = mapper.Map<AddRecordCommand, Domain.Entities.UserRecords.Record>(request);
        record.Id = Guid.NewGuid();
        record.CreatedAt = DateTime.UtcNow;
        record.CreatedBy = request.OwnedByUserId;
        
        List<RecordTagRelation> recordTagRelations = new();

        foreach (var tagId in request.TagIds.Distinct())
        {
            recordTagRelations.Add(RecordTagRelation.CreateInstance(record.Id, tagId, request.OwnedByUserId));
        }
        
        try
        {
            record = await unitOfWork.RecordRepository.Add(record);
            await unitOfWork.RecordTagRelationRepository.AddRange(recordTagRelations);
            await unitOfWork.SaveAsync();

            return request.ReturnSuccessWithObject(mapper.Map<Domain.Entities.UserRecords.Record, RecordDTO>(record));
        }
        catch (Exception e)
        {
            return request.ReturnServerError();
        }
    }
}