using BM2.Shared.DTOs;

namespace BM2.Shared.Requests.Queries.Currency;

public record GetAllCurrenciesQuery : IBaseRequest<IEnumerable<CurrencyDTO>>;