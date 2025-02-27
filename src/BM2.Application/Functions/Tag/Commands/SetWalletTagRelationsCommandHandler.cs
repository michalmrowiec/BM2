using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Responses;
using BM2.Domain.Entities.UserProfile;
using BM2.Shared.DTOs;
using BM2.Shared.Models;
using BM2.Shared.Requests.Commands.Tag;
using BM2.Shared.Requests.Queries.Tag;
using MediatR;

namespace BM2.Application.Functions.Tag.Commands;

public class SetWalletTagRelationsCommandHandler(IMediator mediator, IUnitOfWork unitOfWork)
    : IRequestHandler<SetWalletTagRelationsCommand, BaseResponse<IEnumerable<TagWalletRelationDTO>>>
{
    public async Task<BaseResponse<IEnumerable<TagWalletRelationDTO>>> Handle(
        SetWalletTagRelationsCommand request, CancellationToken cancellationToken)
    {
        var walletTagRelations =
            await unitOfWork.WalletTagRelationRepository.GetAllForUserAsync(request.OwnedByUserId);

        walletTagRelations.ThrowExceptionIfNull();
        walletTagRelations!.CheckPermission(request.OwnedByUserId);

        var toAdd = new List<WalletTagRelation>();
        var toUpdate = new List<WalletTagRelation>();
        var toDelete = new List<WalletTagRelation>();

        foreach (var twr in request.TagWalletRelations)
        {
            var item = walletTagRelations.FirstOrDefault(x =>
                x.TagId == twr.TagId
                && x.WalletId == twr.WalletId);

            if (item == null)
            {
                if (twr.Status != RelationStatus.NotExist)
                {
                    toAdd.Add(WalletTagRelation.CreateInstance(twr.WalletId, twr.TagId, request.OwnedByUserId,
                        twr.Status == RelationStatus.Active));
                }

                continue;
            }

            switch (twr.Status)
            {
                case RelationStatus.Inactive:
                    if (item.IsActive)
                    {
                        item.IsActive = false;
                        toUpdate.Add(item);
                    }

                    break;
                case RelationStatus.Active:
                    if (!item.IsActive)
                    {
                        item.IsActive = true;
                        toUpdate.Add(item);
                    }

                    break;
                case RelationStatus.NotExist:
                    toDelete.Add(item);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        try
        {
            await unitOfWork.WalletTagRelationRepository.AddRange(toAdd);
            await unitOfWork.WalletTagRelationRepository.UpdateRange(toUpdate);
            await unitOfWork.WalletTagRelationRepository.Delete(toDelete);
            await unitOfWork.SaveAsync();

            var result = await mediator.Send(new GetAllTagsForUserWithWalletRelationsQuery(request.OwnedByUserId));

            return request.ReturnSuccessWithObject(result.ReturnedObj ?? []);
        }
        catch (Exception e)
        {
            return request.ReturnServerError();
        }
    }
}