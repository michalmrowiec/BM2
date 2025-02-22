using BM2.Domain.Entities;
using BM2.Domain.Entities.System;
using BM2.Shared.SystemCodes;

namespace BM2.Infrastructure.Seeders;

internal static class RecordStatusSeeder
{
    internal static async Task SeedAsync(BM2DbContext context)
    {
        if (!context.RecordStatuses.Any())
        {
            await context.RecordStatuses.AddRangeAsync(GetRecordStatuses());

            await context.SaveChangesAsync();
        }
    }

    private static List<RecordStatus> GetRecordStatuses()
    {
        return
        [
            new RecordStatus() { Id = Guid.NewGuid(), SystemCode = StatusSystemCode.Planned, RecordStatusName = "Planned", ForRecords = true, ForPeriodicRecord = false },
            new RecordStatus() { Id = Guid.NewGuid(), SystemCode = StatusSystemCode.Realized, RecordStatusName = "Realized", ForRecords = true, ForPeriodicRecord = false },
            new RecordStatus() { Id = Guid.NewGuid(), SystemCode = StatusSystemCode.Active, RecordStatusName = "Active", ForRecords = false, ForPeriodicRecord = true },
            new RecordStatus() { Id = Guid.NewGuid(), SystemCode = StatusSystemCode.Suspended, RecordStatusName = "Suspended", ForRecords = false, ForPeriodicRecord = true },
            new RecordStatus() { Id = Guid.NewGuid(), SystemCode = StatusSystemCode.Pending, RecordStatusName = "Pending", ForRecords = true, ForPeriodicRecord = false }
        ];
    }
}