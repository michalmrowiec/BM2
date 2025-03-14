﻿using AutoMapper;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Responses;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Queries.Currency;
using MediatR;

namespace BM2.Application.Functions.Currency.Queries;

public class GetAllCurrenciesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetAllCurrenciesQuery, BaseResponse<IEnumerable<CurrencyDTO>>>
{
    public async Task<BaseResponse<IEnumerable<CurrencyDTO>>> Handle(GetAllCurrenciesQuery request,
        CancellationToken cancellationToken)
    {
        return request.ReturnSuccessWithObject(
            mapper.Map<IEnumerable<CurrencyDTO>>(await unitOfWork.CurrencyRepository.GetAllAsync()));
    }
}