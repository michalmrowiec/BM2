using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Responses;
using BM2.Domain.Entities.UserProfile;
using BM2.Shared.DTOs;
using BM2.Shared.Models;
using BM2.Shared.Requests.Commands.Category;
using BM2.Shared.Requests.Queries.Category;
using MediatR;

namespace BM2.Application.Functions.Category.Commands;

public class SetWalletCategoryRelationsCommandHandler(IMediator mediator, IUnitOfWork unitOfWork)
    : IRequestHandler<SetWalletCategoryRelationsCommand, BaseResponse<IEnumerable<CategoryWalletRelationDTO>>>
{
    public async Task<BaseResponse<IEnumerable<CategoryWalletRelationDTO>>> Handle(
        SetWalletCategoryRelationsCommand request, CancellationToken cancellationToken)
    {
        var walletCategoryRelations =
            await unitOfWork.WalletCategoryRelationRepository.GetAllForUserAsync(request.UserId);

        walletCategoryRelations.ThrowExceptionIfNull();
        walletCategoryRelations!.CheckPermission(request.UserId);

        var toAdd = new List<WalletCategoryRelation>();
        var toUpdate = new List<WalletCategoryRelation>();
        var toDelete = new List<WalletCategoryRelation>();

        foreach (var cwr in request.CategoryWalletRelations)
        {
            var item = walletCategoryRelations.FirstOrDefault(x =>
                x.CategoryId == cwr.CategoryId
                && x.WalletId == cwr.WalletId);

            if (item == null)
            {
                if (cwr.Status != RelationStatus.NotExist)
                {
                    toAdd.Add(WalletCategoryRelation.CreateInstance(cwr.WalletId, cwr.CategoryId, request.UserId,
                        cwr.Status == RelationStatus.Active));
                }

                continue;
            }

            switch (cwr.Status)
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
            await unitOfWork.WalletCategoryRelationRepository.AddRange(toAdd);
            await unitOfWork.WalletCategoryRelationRepository.UpdateRange(toUpdate);
            await unitOfWork.WalletCategoryRelationRepository.Delete(toDelete);
            await unitOfWork.SaveAsync();

            var result = await mediator.Send(new GetAllCategoriesForUserWithWalletRelationsQuery(request.UserId));

            return request.ReturnSuccessWithObject(result.ReturnedObj ?? []);
        }
        catch (Exception e)
        {
            return request.ReturnServerError();
        }
    }
}