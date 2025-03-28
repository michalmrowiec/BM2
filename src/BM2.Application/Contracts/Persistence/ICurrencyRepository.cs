﻿using BM2.Application.Contracts.Persistence.Base;
using BM2.Domain.Entities;
using BM2.Domain.Entities.System;

namespace BM2.Application.Contracts.Persistence;

public interface ICurrencyRepository
{
    Task<IReadOnlyList<Currency>> GetAllAsync();
}