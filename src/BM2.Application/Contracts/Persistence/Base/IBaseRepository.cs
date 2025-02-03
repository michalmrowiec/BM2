using System.Linq.Expressions;

namespace BM2.Application.Contracts.Persistence.Base;

public interface IBaseRepository<T> : ICrudRepository<T>
{
    Task<T?> GetByIdAsync(Guid id);
    Task<T?> GetByAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
    //Task<T> GetByIdAsync(Expression<Func<T, bool>> predicate);
    //Task<T> GetByIdAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
    Task<IReadOnlyList<T>> GetAllAsync();
    
}