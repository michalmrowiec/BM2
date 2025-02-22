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
            return entity;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<IEnumerable<T>> AddRange(IList<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
        return entities;
    }

    public Task<T> Update(T entity)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<T>> UpdateRange(IList<T> entities)
    {
        _dbSet.UpdateRange(entities);
        return await Task.FromResult(entities);
    }

    public Task Delete(params IList<T> entity)
    {
        _dbSet.RemoveRange(entity);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<T>> GetAllForUserAsync(Guid userId,
        params Func<IQueryable<T>, IQueryable<T>>[] includes)
    {
        IQueryable<T> query = _dbSet.Where(x => ((IOwnedByUser)x).OwnedByUserId == userId);

        query = ApplyIncludes(query, includes);

        query = SoftDeleteFilter(query);

        return await query.ToListAsync();
    }

    public async Task Save()
    {
        await context.SaveChangesAsync();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<T?> GetByIdAsync(Guid id, params Func<IQueryable<T>, IQueryable<T>>[] includes)
    {
        IQueryable<T> query = _dbSet.Where(x => x.Id == id);

        query = ApplyIncludes(query, includes);

        query = SoftDeleteFilter(query);

        return await query.FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<T>> GetByIdsAsync(IList<Guid> ids)
    {
        if (ids.Any())
            return new List<T>();

        return await _dbSet.Where(x => ids.Contains(x.Id)).ToListAsync();
    }

    public async Task<T?> GetByAsync(Expression<Func<T, bool>> predicate,
        params Func<IQueryable<T>, IQueryable<T>>[] includes)
    {
        var results = await GetListByAsync(predicate, includes);
        return results.FirstOrDefault();
    }

    public async Task<IReadOnlyList<T>> GetListByAsync(Expression<Func<T, bool>> predicate,
        params Func<IQueryable<T>, IQueryable<T>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        query = ApplyIncludes(query, includes);

        query = SoftDeleteFilter(query);

        return await query.Where(predicate).ToListAsync();
    }

    public async Task<IReadOnlyList<T>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    // private IQueryable<T> ApplyIncludes(IQueryable<T> query, params Expression<Func<T, object>>[] includes)
    // {
    //     foreach (var include in includes)
    //     {
    //         query = query.Include(include);
    //     }
    //     
    //     return query;
    // }

    private IQueryable<T> ApplyIncludes(IQueryable<T> query, params Func<IQueryable<T>, IQueryable<T>>[] includes)
    {
        foreach (var include in includes)
        {
            query = include(query);
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