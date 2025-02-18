using AutoMapper;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Responses;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Queries.Account;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BM2.Application.Functions.Account.Queries;

public class GetAllAccountsForUserQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllAccountsForUserQuery, BaseResponse<IEnumerable<AccountDTO>>>
{
    public async Task<BaseResponse<IEnumerable<AccountDTO>>> Handle(GetAllAccountsForUserQuery request,
        CancellationToken cancellationToken)
    {
        var accounts = await unitOfWork.AccountRepository.GetAllForUserAsync(request.UserId,
            q => q.Include(a => a.DefaultCurrency),
            q => q.Include(a => a.Wallet));

        accounts.ThrowExceptionIfNull();
        accounts!.CheckPermission(request.UserId);

        return request.ReturnSuccessWithObject(mapper.Map<IEnumerable<AccountDTO>>(accounts));
    }
}