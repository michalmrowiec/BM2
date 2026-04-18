using AutoMapper;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Functions.Record.Commands.Validators;
using BM2.Application.Responses;
using BM2.Domain.Entities.UserRecords;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.Record;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BM2.Application.Functions.Record.Commands;

public class AddRecordTemplateCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
    : IRequestHandler<AddRecordTemplateCommand, BaseResponse<RecordTemplateDTO>>
{
    public async Task<BaseResponse<RecordTemplateDTO>> Handle(AddRecordTemplateCommand request, CancellationToken cancellationToken)
    {
        var validationResult =
            await new AddRecordTemplateCommandValidator(unitOfWork).ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid) return new BaseResponse<RecordTemplateDTO>(validationResult);

        var recordTemplate = mapper.Map<AddRecordTemplateCommand, Domain.Entities.UserRecords.RecordTemplate>(request);
        recordTemplate.Id = Guid.NewGuid();
        recordTemplate.CreatedAt = DateTime.UtcNow;
        recordTemplate.CreatedBy = request.OwnedByUserId;
        
        List<RecordTagRelation> recordTagRelations = new();

        foreach (var tagId in request.TagIds.Distinct())
        {
            recordTagRelations.Add(RecordTagRelation.CreateInstance(recordTemplate.Id, tagId, request.OwnedByUserId));
        }
        
        try
        {
            await unitOfWork.RecordTemplateRepository.Add(recordTemplate);
            await unitOfWork.RecordTagRelationRepository.AddRange(recordTagRelations);
            await unitOfWork.SaveAsync();

            var createdRecordTemplate = await unitOfWork.RecordTemplateRepository.GetByIdAsync(recordTemplate.Id,
                q => q.Include(x => x.Wallet),
                q => q.Include(x => x.Currency),
                q => q.Include(x => x.Category),
                q => q.Include(x => x.Status),
                q => q.Include(x => x.Tags));

            createdRecordTemplate.ThrowExceptionIfNull();

            return request.ReturnSuccessWithObject(mapper.Map<Domain.Entities.UserRecords.RecordTemplate, RecordTemplateDTO>(createdRecordTemplate));
        }
        catch (Exception)
        {
            return request.ReturnServerError();
        }
    }
}
