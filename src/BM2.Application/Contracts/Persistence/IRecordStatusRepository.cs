using BM2.Application.Contracts.Persistence.Base;
using BM2.Domain.Entities.System;

namespace BM2.Application.Contracts.Persistence;

public interface IRecordStatusRepository : IGenericRepository<RecordStatus>
{
    Task<IReadOnlyList<RecordStatus>> GetStatusesForRecords();
    Task<IReadOnlyList<RecordStatus>> GetStatusesForPeriodicRecord();
}
