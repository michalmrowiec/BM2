using System.Linq.Expressions;

namespace BM2.Application.Contracts.Persistence.Base;

public interface ICrudRepository<T> : IAddAsync<T>, IDeleteAsync<T>
{
    Task<T> UpdateAsync(T entity);
}