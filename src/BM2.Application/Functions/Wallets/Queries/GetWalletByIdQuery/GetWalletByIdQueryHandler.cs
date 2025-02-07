using AutoMapper;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.DTOs;
using BM2.Application.Responses;
using MediatR;

namespace BM2.Application.Functions.Wallets.Queries.GetWalletByIdQuery;

public class GetWalletByIdQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
    : IRequestHandler<GetWalletByIdQuery, BaseResponse<WalletDTO>>
{
    public async Task<BaseResponse<WalletDTO>> Handle(GetWalletByIdQuery request, CancellationToken cancellationToken)
    {
        var wallet = await unitOfWork.WalletRepository.GetByIdAsync(request.WalletId);

        wallet.ThrowExceptionIfNull();
        wallet!.CheckPermission(request.UserId);

        return request.ReturnSuccessWithObject(mapper.Map<WalletDTO>(wallet));
    }
}