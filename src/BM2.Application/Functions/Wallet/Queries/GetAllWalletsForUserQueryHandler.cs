using AutoMapper;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Responses;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Wallet;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BM2.Application.Functions.Wallet.Queries;

public class GetAllWalletsForUserQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllWalletsForUserQuery, BaseResponse<IEnumerable<WalletDTO>>>
{
    public async Task<BaseResponse<IEnumerable<WalletDTO>>> Handle(GetAllWalletsForUserQuery request,
        CancellationToken cancellationToken)
    {
        var wallets =
            await unitOfWork.WalletRepository.GetAllForUserAsync(request.UserId,
                q => q.Include(w => w.DefaultCurrency));

        wallets.ThrowExceptionIfNull();
        wallets!.CheckPermission(request.UserId);

        return request.ReturnSuccessWithObject(mapper.Map<IEnumerable<WalletDTO>>(wallets));
    }
}