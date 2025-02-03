using System.Linq.Expressions;
using BM2.Application.Contracts.Persistence;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BM2.Infrastructure.Repositories;

public class BaseRepository<T>(
    BM2DbContext context,
    ILogger<BaseRepository<T>> logger) : IBaseRepository<T> where T : class
{
    private readonly DbSet<T> _dbSet = context.Set<T>();

    public async Task<T> AddAsync(T entity)
    {
        try
        {
            await _dbSet.AddAsync(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating entity");
            throw;
        }
    }

    public Task<T> UpdateAsync(T entity)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(T entity)
    {
        throw new NotImplementedException();
    }

    public Task<T?> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<T?> GetByAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        if (typeof(IEntityAudit).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(e => ((IEntityAudit)e).DeletedAt == null);
        }

        return await query.FirstOrDefaultAsync(predicate);
    }

    public Task<IReadOnlyList<T>> GetAllAsync()
    {
        throw new NotImplementedException();
    }
}