using BM2.Domain.Entities;

namespace BM2.Infrastructure.Repositories;

public static class BaseRepository
{
    internal static IQueryable<T> GetUndeleted <T>(this IQueryable<T> query) where T : class, IEntity, IEntityAudit
    {
        return query.Where(x => x.DeletedAt == null);
    }
}