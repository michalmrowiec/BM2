using BM2.Application.Mappings;
using BM2.Infrastructure.Repositories.Base;
using BM2.Application.Responses;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Queries.Account;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BM2.Application.Functions.Account.Queries;

public class GetAllAccountsForUserQueryHandler(UnitOfWork unitOfWork)
    : IRequestHandler<GetAllAccountsForUserQuery, BaseResponse<IEnumerable<AccountDTO>>>
{
    public async Task<BaseResponse<IEnumerable<AccountDTO>>> Handle(GetAllAccountsForUserQuery request,
        CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Entities.UserProfile.Account> accounts;

        if (request.ActiveOnly)
            accounts = await unitOfWork.AccountRepository.GetActiveForUserAsync(request.UserId,
                q => q.Include(a => a.Wallet).Include(a => a.DefaultCurrency));
        else
            accounts = await unitOfWork.AccountRepository.GetAllForUserAsync(request.UserId,
                q => q.Include(a => a.DefaultCurrency).Include(a => a.Wallet));

        accounts.ThrowExceptionIfNull();
        accounts!.CheckPermission(request.UserId);

        return request.ReturnSuccessWithObject(accounts.Select(x => x.ToDto()));
    }
}