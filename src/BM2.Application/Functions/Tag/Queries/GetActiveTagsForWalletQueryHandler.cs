using AutoMapper;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Responses;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Queries.Tag;
using MediatR;

namespace BM2.Application.Functions.Tag.Queries;

public class GetActiveTagsForWalletQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetActiveTagsForWalletQuery, BaseResponse<IEnumerable<TagDTO>>>
{
    public async Task<BaseResponse<IEnumerable<TagDTO>>> Handle(GetActiveTagsForWalletQuery request,
        CancellationToken cancellationToken)
    {
        var items =
            await unitOfWork.TagRepository.GetTagsForWalletAsync(request.UserId, request.WalletId, true);

        items!.CheckPermission(request.UserId);

        return request.ReturnSuccessWithObject(mapper.Map<IEnumerable<TagDTO>>(items));
    }
}