using BM2.Domain.Entities.System;

namespace BM2.Application.Contracts.Persistence;

public interface IRecordStatusRepository
{
    Task<IReadOnlyList<RecordStatus>> GetStatusesForRecords();
    Task<IReadOnlyList<RecordStatus>> GetStatusesForPeriodicRecord();
}