using BM2.Application.Mappings;
using BM2.Infrastructure.Repositories.Base;
using BM2.Application.Responses;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Queries.Category;
using MediatR;

namespace BM2.Application.Functions.Category.Queries;

public class GetAllCategoriesForUserQueryHandler(UnitOfWork unitOfWork)
    : IRequestHandler<GetAllCategoriesForUserQuery, BaseResponse<IEnumerable<CategoryDTO>>>
{
    public async Task<BaseResponse<IEnumerable<CategoryDTO>>> Handle(GetAllCategoriesForUserQuery request,
        CancellationToken cancellationToken)
    {
        var items =
            await unitOfWork.CategoryRepository.GetAllForUserAsync(request.UserId);

        items!.CheckPermission(request.UserId);

        return request.ReturnSuccessWithObject(items.Select(x => x.ToDto()));
    }
}