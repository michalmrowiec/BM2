using AutoMapper;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Functions.Tag.Commands.Validators;
using BM2.Application.Responses;
using BM2.Domain.Entities.UserProfile;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.Tag;
using MediatR;

namespace BM2.Application.Functions.Tag.Commands;

public class AddTagCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
    : IRequestHandler<AddTagCommand, BaseResponse<TagDTO>>
{
    public async Task<BaseResponse<TagDTO>> Handle(AddTagCommand request, CancellationToken cancellationToken)
    {
        var validationResult =
            await new AddTagCommandValidator(unitOfWork).ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid) return new BaseResponse<TagDTO>(validationResult);

        var tag = mapper.Map<AddTagCommand, Domain.Entities.UserProfile.Tag>(request);
        tag.Id = Guid.NewGuid();
        tag.CreatedAt = DateTime.UtcNow;
        tag.CreatedBy = request.OwnedByUserId;

        List<WalletTagRelation> walletTagRelations = new();

        foreach (var walletId in request.WalletIds.Distinct())
        {
            walletTagRelations.Add(WalletTagRelation.CreateInstance(walletId, tag.Id, request.OwnedByUserId));
        }
        
        try
        {
            tag = await unitOfWork.TagRepository.Add(tag);
            await unitOfWork.WalletTagRelationRepository.AddRange(walletTagRelations);
            await unitOfWork.SaveAsync();

            return request.ReturnSuccessWithObject(mapper.Map<Domain.Entities.UserProfile.Tag, TagDTO>(tag));
        }
        catch (Exception e)
        {
            return request.ReturnServerError();
        }
    }
}