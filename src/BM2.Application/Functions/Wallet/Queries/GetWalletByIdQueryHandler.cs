using AutoMapper;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Responses;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Wallet;
using MediatR;

namespace BM2.Application.Functions.Wallet.Queries;

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