using AutoMapper;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Responses;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Queries.Category;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BM2.Application.Functions.Category.Queries;

public class GetAllCategoriesForUserWithWalletRelationsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetAllCategoriesForUserWithWalletRelationsQuery,
        BaseResponse<IEnumerable<CategoryWithWalletRelationDTO>>>
{
    public async Task<BaseResponse<IEnumerable<CategoryWithWalletRelationDTO>>> Handle(
        GetAllCategoriesForUserWithWalletRelationsQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await unitOfWork.CategoryRepository.GetAllForUserAsync(request.UserId);
        var wallets =
            await unitOfWork.WalletRepository.GetAllForUserAsync(request.UserId, q => q.Include(w => w.Categories));

        categories.ThrowExceptionIfNull();
        categories!.CheckPermission(request.UserId);

        wallets.ThrowExceptionIfNull();
        wallets!.CheckPermission(request.UserId);

        IList<CategoryWithWalletRelationDTO> categoriesDto = new List<CategoryWithWalletRelationDTO>();

        foreach (var category in categories)
        {
            var c = new CategoryWithWalletRelationDTO()
            {
                Id = category.Id,
                CategoryName = category.CategoryName,
                WalletRelations = new List<WalletCategoryRelationDTO>()
            };

            foreach (var wallet in wallets)
            {
                c.WalletRelations.Add(
                    new()
                        { WalletId = wallet.Id, RelationExists = wallet.Categories.Contains(category) });
            }
            categoriesDto.Add(c);
        }

        return request.ReturnSuccessWithObject(categoriesDto);
    }
}