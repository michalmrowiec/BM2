using BM2.Domain.Entities;

namespace BM2.Infrastructure.Seeders;

internal static class RecordStatusSeeder
{
    internal static async Task Seed(this BM2DbContext context)
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
            new RecordStatus() { RecordStatusId = Guid.NewGuid(), RecordStatusName = "Planned", ForRecords = true, ForPeriodicRecord = false },
            new RecordStatus() { RecordStatusId = Guid.NewGuid(), RecordStatusName = "Realized", ForRecords = true, ForPeriodicRecord = false},
            new RecordStatus() { RecordStatusId = Guid.NewGuid(), RecordStatusName = "Active", ForRecords = false, ForPeriodicRecord = true}, //ON
            new RecordStatus() { RecordStatusId = Guid.NewGuid(), RecordStatusName = "Suspended", ForRecords = false, ForPeriodicRecord = true} //OFF
        ];
    }
}