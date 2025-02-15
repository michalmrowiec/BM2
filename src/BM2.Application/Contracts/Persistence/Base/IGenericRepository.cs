using System.Linq.Expressions;

namespace BM2.Application.Contracts.Persistence.Base;

public interface IGenericRepository<T>
{
    Task<T> Add(T entity);
    Task<IEnumerable<T>> AddRange(IList<T> entities);
    Task<T> Update(T entity);
    Task Delete(T entity);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<IReadOnlyList<T>> GetAllForUserAsync(Guid userId, params Expression<Func<T, object>>[] includes);
    Task<T?> GetByIdAsync(Guid id);
    Task<T?> GetByIdAsync(Guid id, params Func<IQueryable<T>, IQueryable<T>>[] includes);
    Task<IReadOnlyList<T>> GetByIdsAsync(IList<Guid> ids);
    protected Task<T?> GetByAsync(Expression<Func<T, bool>> predicate, params Func<IQueryable<T>, IQueryable<T>>[] includes);
    protected Task<IReadOnlyList<T>> GetListByAsync(Expression<Func<T, bool>> predicate, params Func<IQueryable<T>, IQueryable<T>>[] includes);

    //Task<T> GetByIdAsync(Expression<Func<T, bool>> predicate);
    //Task<T> GetByIdAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
}