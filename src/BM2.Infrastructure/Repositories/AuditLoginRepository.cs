using BM2.Application.Contracts.Persistence;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace BM2.Infrastructure.Repositories;

public class AuditLoginRepository(
    BM2DbContext context,
    IBaseRepository<AuditLogin> repository,
    ILogger<AuditLoginRepository> logger) : IAuditLoginRepository
{
    public async Task<AuditLogin> AddAsync(AuditLogin auditLogin) => await repository.AddAsync(auditLogin);
}