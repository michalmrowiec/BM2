using BM2.Domain.Entities;
using BM2.Domain.Models;

namespace BM2.Application.Contracts.Persistence;

public interface IAuditLoginRepository
{
    Task<AuditLogin> AddAsync(AuditLogin auditLogin);
}