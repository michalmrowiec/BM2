using AutoMapper;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.DTOs;
using BM2.Application.Functions.Wallets.Queries.Requests;
using BM2.Application.Responses;
using MediatR;

namespace BM2.Application.Functions.Wallets.Queries.Handlers;

public class GetAllWalletsForUserQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllWalletsForUserQuery, BaseResponse<IEnumerable<WalletDTO>>>
{
    public async Task<BaseResponse<IEnumerable<WalletDTO>>> Handle(GetAllWalletsForUserQuery request,
        CancellationToken cancellationToken)
    {
        var wallets = await unitOfWork.WalletRepository.GetAllForUserAsync(request.UserId);

        wallets.ThrowExceptionIfNull();
        wallets!.CheckPermission(request.UserId);

        return request.ReturnSuccessWithObject(mapper.Map<IEnumerable<WalletDTO>>(wallets));
    }
}