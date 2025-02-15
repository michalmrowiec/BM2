using BM2.Application.Contracts.Persistence;
using BM2.Domain.Entities;
using BM2.Domain.Entities.System;
using BM2.Infrastructure.Repositories.Base;

namespace BM2.Infrastructure.Repositories;

public class RecordStatusRepository(
    BM2DbContext context) : GenericRepository<RecordStatus>(context), IRecordStatusRepository
{
    public async Task<IReadOnlyList<RecordStatus>> GetStatusesForRecords() =>
        await GetListByAsync(x => x.ForRecords);

    public async Task<IReadOnlyList<RecordStatus>> GetStatusesForPeriodicRecord() =>
        await GetListByAsync(x => x.ForPeriodicRecord);
}