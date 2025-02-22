using AutoMapper;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Responses;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Queries.Category;
using MediatR;

namespace BM2.Application.Functions.Category.Queries;

public class GetActiveCategoriesForWalletQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetActiveCategoriesForWalletQuery, BaseResponse<IEnumerable<CategoryDTO>>>
{
    public async Task<BaseResponse<IEnumerable<CategoryDTO>>> Handle(GetActiveCategoriesForWalletQuery request,
        CancellationToken cancellationToken)
    {
        var items =
            await unitOfWork.CategoryRepository.GetCategoryForWalletAsync(request.UserId, request.WalletId, true);

        items.ThrowExceptionIfNull();
        items!.CheckPermission(request.UserId);

        return request.ReturnSuccessWithObject(mapper.Map<IEnumerable<CategoryDTO>>(items));
    }
}