using System.Linq.Expressions;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Domain.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BM2.Infrastructure.Repositories.Base;

public class GenericRepository<T>(
    BM2DbContext context) : IGenericRepository<T> where T : class, IEntity
{
    private readonly DbSet<T> _dbSet = context.Set<T>();

    public async Task<T> Add(T entity)
    {
        try
        {
            await _dbSet.AddAsync(entity);
            // await context.SaveChangesAsync();
            return entity;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public Task<T> Update(T entity)
    {
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyList<T>> GetAllForUserAsync(Guid userId, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet.Where(x => ((IOwnedByUser)x).OwnedByUserId == userId);

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        if (typeof(IEntityAudit).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(x => ((IEntityAudit)x).DeletedAt == null);
        }

        return await query.ToListAsync();
    }

    public Task Delete(T entity)
    {
        throw new NotImplementedException();
    }

    public async Task Save()
    {
        await context.SaveChangesAsync();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<T?> GetByIdAsync(Guid id, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet.Where(x => x.Id == id);

        query = ApplyIncludes(query, includes);

        query = SoftDeleteFilter(query);

        return await query.FirstOrDefaultAsync();
    }

    public async Task<T?> GetByAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        query = ApplyIncludes(query, includes);
        // foreach (var include in includes)
        // {
        //     query = query.Include(include);
        // }

        query = SoftDeleteFilter(query);
        // if (typeof(IEntityAudit).IsAssignableFrom(typeof(T)))
        // {
        //     query = query.Where(e => ((IEntityAudit)e).DeletedAt == null);
        // }

        return await query.FirstOrDefaultAsync(predicate);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    private IQueryable<T> ApplyIncludes(IQueryable<T> query, params Expression<Func<T, object>>[] includes)
    {
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        
        return query;
    }

    private IQueryable<T> SoftDeleteFilter(IQueryable<T> query)
    {
        if (typeof(IEntityAudit).IsAssignableFrom(typeof(T)))
        {
            //query = query.Where(e => ((IEntityAudit)e).DeletedAt == null);
            query = query.Cast<IEntityAudit>().Where(e => e.DeletedAt == null).Cast<T>();
        }

        return query;
    }
}