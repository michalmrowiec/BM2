using BM2.Application.Functions.DTOs;
using BM2.Domain.Entities;

namespace BM2.Application.Functions.Requests.Query;

public record GetAllCurrenciesQuery : IBaseRequest<IEnumerable<CurrencyDTO>>;