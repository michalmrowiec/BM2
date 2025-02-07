using BM2.Application.DTOs;

namespace BM2.Application.Functions.Curencies.Queries.GetAllCurrenciesQuery;

public record GetAllCurrenciesQuery : IBaseRequest<IEnumerable<CurrencyDTO>>;