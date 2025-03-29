using BM2.Application.Contracts.Persistence.Base;
using BM2.Domain.Entities.UserRecords;

namespace BM2.Application.Contracts.Persistence;

public interface IRecordRepository : IGenericRepository<Record>
{
    Task<IReadOnlyList<Record>> GetAllForMonthAsync(Guid userId, int year, int month, Guid? walletId = null);
}