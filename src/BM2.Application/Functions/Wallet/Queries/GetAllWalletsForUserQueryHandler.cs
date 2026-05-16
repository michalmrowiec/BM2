using BM2.Application.Mappings;
using BM2.Infrastructure.Repositories.Base;
using BM2.Application.Responses;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Wallet;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BM2.Application.Functions.Wallet.Queries;

public class GetAllWalletsForUserQueryHandler(UnitOfWork unitOfWork)
    : IRequestHandler<GetAllWalletsForUserQuery, BaseResponse<IEnumerable<WalletDTO>>>
{
    public async Task<BaseResponse<IEnumerable<WalletDTO>>> Handle(GetAllWalletsForUserQuery request,
        CancellationToken cancellationToken)
    {
        var wallets =
            await unitOfWork.WalletRepository.GetAllForUserAsync(request.UserId,
                q => q.Include(w => w.DefaultCurrency),
                q => q.Include(w => w.Accounts));

        wallets.ThrowExceptionIfNull();
        wallets!.CheckPermission(request.UserId);

        return request.ReturnSuccessWithObject(wallets.Select(x => x.ToDto()));
    }
}