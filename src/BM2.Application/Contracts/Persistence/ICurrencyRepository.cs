using BM2.Application.Contracts.Persistence.Base;
using BM2.Domain.Entities;

namespace BM2.Application.Contracts.Persistence;

public interface ICurrencyRepository
{
    Task<IReadOnlyList<Currency>> GetAllAsync();
}