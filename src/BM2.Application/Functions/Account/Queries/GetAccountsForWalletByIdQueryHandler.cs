using AutoMapper;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Responses;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Queries.Account;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BM2.Application.Functions.Account.Queries;

public class GetAccountsForWalletByIdQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
    : IRequestHandler<GetAccountsForWalletByIdQuery, BaseResponse<IEnumerable<AccountDTO>>>
{
    public async Task<BaseResponse<IEnumerable<AccountDTO>>> Handle(GetAccountsForWalletByIdQuery request,
        CancellationToken cancellationToken)
    {
        var wallet = await unitOfWork.WalletRepository.GetByIdAsync(request.WalletId,
            q => q.Include(w => w.Accounts).ThenInclude(a => a.DefaultCurrency));

        wallet.ThrowExceptionIfNull();
        wallet!.CheckPermission(request.UserId);

        var accounts = wallet!.Accounts;
        accounts.CheckPermission(request.UserId);

        return request.ReturnSuccessWithObject(mapper.Map<IEnumerable<AccountDTO>>(accounts));
    }
}