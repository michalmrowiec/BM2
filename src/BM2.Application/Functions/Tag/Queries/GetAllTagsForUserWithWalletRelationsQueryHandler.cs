using AutoMapper;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Responses;
using BM2.Shared.DTOs;
using BM2.Shared.Models;
using BM2.Shared.Requests.Queries.Tag;
using MediatR;

namespace BM2.Application.Functions.Tag.Queries;

public class GetAllTagsForUserWithWalletRelationsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetAllTagsForUserWithWalletRelationsQuery,
        BaseResponse<IEnumerable<TagWalletRelationDTO>>>
{
    public async Task<BaseResponse<IEnumerable<TagWalletRelationDTO>>> Handle(
        GetAllTagsForUserWithWalletRelationsQuery request,
        CancellationToken cancellationToken)
    {
        var tags = await unitOfWork.TagRepository.GetAllForUserAsync(request.UserId);
        var walletTagRelations =
            await unitOfWork.WalletTagRelationRepository.GetAllForUserAsync(request.UserId);
        var wallets = await unitOfWork.WalletRepository.GetAllForUserAsync(request.UserId);

        tags.CheckPermission(request.UserId);

        walletTagRelations.CheckPermission(request.UserId);

        wallets.CheckPermission(request.UserId);

        List<TagWalletRelationDTO> tagWalletRelations = [];

        foreach (var tag in tags)
        {
            var twr = new TagWalletRelationDTO()
            {
                Id = tag.Id,
                TagName = tag.TagName,
                WalletRelations = new List<WalletRelationDTO>()
            };

            foreach (var wallet in wallets)
            {
                var status = RelationStatus.NotExist;

                var thisRelation =
                    walletTagRelations.FirstOrDefault(x => x.WalletId == wallet.Id && x.TagId == tag.Id);
                if (thisRelation != null)
                    status = thisRelation.IsActive ? RelationStatus.Active : RelationStatus.Inactive;

                twr.WalletRelations.Add(new WalletRelationDTO()
                {
                    WalletId = wallet.Id,
                    Status = status
                });
            }

            tagWalletRelations.Add(twr);
        }

        return request.ReturnSuccessWithObject(tagWalletRelations);


        // var wallets =
        //     await unitOfWork.WalletRepository.GetAllForUserAsync(request.UserId, q => q.Include(w => w.Categories));
        //
        // categories.ThrowExceptionIfNull();
        // categories!.CheckPermission(request.UserId);
        //
        // wallets.ThrowExceptionIfNull();
        // wallets!.CheckPermission(request.UserId);
        //
        // IList<CategoryWithWalletRelationDTO> categoriesDto = new List<CategoryWithWalletRelationDTO>();
        //
        // foreach (var category in categories)
        // {
        //     var c = new CategoryWithWalletRelationDTO()
        //     {
        //         Id = category.Id,
        //         CategoryName = category.CategoryName,
        //         WalletRelations = new List<WalletCategoryRelationDTO>()
        //     };
        //
        //     foreach (var wallet in wallets)
        //     {
        //
        //         c.WalletRelations.Add(
        //             new()
        //             {
        //                 WalletId = wallet.Id,
        //                 IsActive = wallet.Categories.Contains(category)
        //             });
        //     }
        //     categoriesDto.Add(c);
        // }
        //
        // return request.ReturnSuccessWithObject(categoriesDto);
    }
}