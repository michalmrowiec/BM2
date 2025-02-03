namespace BM2.Application.Contracts.Persistence.Base;

public interface IDeleteAsync<T>
{
    Task DeleteAsync(T entity);
}