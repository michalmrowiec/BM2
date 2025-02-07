using BM2.Application.Contracts.Persistence;
using BM2.Domain.Entities;
using BM2.Domain.Entities.System;
using BM2.Infrastructure.Repositories.Base;

namespace BM2.Infrastructure.Repositories;

public class CurrencyRepository(
    BM2DbContext context) : GenericRepository<Currency>(context), ICurrencyRepository
{
}