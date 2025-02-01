using BM2.Application.Contracts.Persistence;
using BM2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;

namespace BM2.Infrastructure.Repositories;

public class AuditLoginRepository(
    BM2DbContext context,
    ILogger<AuditLoginRepository> logger) : IAuditLoginRepository
{
    public async Task<AuditLogin> AddAsync(AuditLogin auditLogin)
    {
        try
        {
            await context.AuditLogins.AddAsync(auditLogin);
            await context.SaveChangesAsync();
            return auditLogin;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating entity");
            throw;
        }
    }
}