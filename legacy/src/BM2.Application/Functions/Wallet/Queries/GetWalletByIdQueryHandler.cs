using BM2.Application.Mappings;
using BM2.Infrastructure.Repositories.Base;
using BM2.Application.Responses;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Wallet;
using MediatR;

namespace BM2.Application.Functions.Wallet.Queries;

public class GetWalletByIdQueryHandler(UnitOfWork unitOfWork)
    : IRequestHandler<GetWalletByIdQuery, BaseResponse<WalletDTO>>
{
    public async Task<BaseResponse<WalletDTO>> Handle(GetWalletByIdQuery request, CancellationToken cancellationToken)
    {
        var wallet = await unitOfWork.WalletRepository.GetByIdAsync(request.WalletId);

        wallet.ThrowExceptionIfNull();
        wallet!.CheckPermission(request.UserId);

        return request.ReturnSuccessWithObject(wallet.ToDto());
    }
}