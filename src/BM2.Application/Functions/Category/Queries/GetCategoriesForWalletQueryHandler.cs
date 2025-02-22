using AutoMapper;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Responses;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Queries.Category;
using MediatR;

namespace BM2.Application.Functions.Category.Queries;

public class GetCategoriesForWalletQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetCategoriesForWalletQuery, BaseResponse<IEnumerable<CategoryDTO>>>
{
    public async Task<BaseResponse<IEnumerable<CategoryDTO>>> Handle(GetCategoriesForWalletQuery request,
        CancellationToken cancellationToken)
    {
        var items =
            await unitOfWork.CategoryRepository.GetCategoryForWalletAsync(request.UserId, request.WalletId);

        items.ThrowExceptionIfNull();
        items!.CheckPermission(request.UserId);

        return request.ReturnSuccessWithObject(mapper.Map<IEnumerable<CategoryDTO>>(items));
    }
}