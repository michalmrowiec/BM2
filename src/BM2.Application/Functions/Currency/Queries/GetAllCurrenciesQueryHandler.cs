using BM2.Application.Mappings;
using BM2.Infrastructure.Repositories.Base;
using BM2.Application.Responses;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Queries.Currency;
using MediatR;

namespace BM2.Application.Functions.Currency.Queries;

public class GetAllCurrenciesQueryHandler(UnitOfWork unitOfWork)
    : IRequestHandler<GetAllCurrenciesQuery, BaseResponse<IEnumerable<CurrencyDTO>>>
{
    public async Task<BaseResponse<IEnumerable<CurrencyDTO>>> Handle(GetAllCurrenciesQuery request,
        CancellationToken cancellationToken)
    {
        return request.ReturnSuccessWithObject(
            (await unitOfWork.CurrencyRepository.GetAllAsync()).Select(x => x.ToDto()));
    }
}