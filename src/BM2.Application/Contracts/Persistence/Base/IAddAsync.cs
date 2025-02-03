namespace BM2.Application.Contracts.Persistence.Base;

public interface IAddAsync<T>
{
    Task<T> AddAsync(T entity);
}