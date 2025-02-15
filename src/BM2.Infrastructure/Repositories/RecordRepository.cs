using BM2.Application.Contracts.Persistence;
using BM2.Domain.Entities.UserRecords;
using BM2.Infrastructure.Repositories.Base;

namespace BM2.Infrastructure.Repositories;

public class RecordRepository(
    BM2DbContext context) : GenericRepository<Record>(context), IRecordRepository
{
    public async Task<IReadOnlyList<Record>> GetAllForMonthAsync(Guid userId, int year, int month) =>
        await GetListByAsync(x =>
            x.OwnedByUserId == userId
            && x.RecordDateTime.Year == year
            && x.RecordDateTime.Month == month);
}