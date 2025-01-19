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
            new RecordStatus()
            {
                Id = Guid.NewGuid(), StatusCode = 1, RecordStatusName = "Planned", ForRecords = true,
                ForPeriodicRecord = false
            },
            new RecordStatus()
            {
                Id = Guid.NewGuid(), StatusCode = 2, RecordStatusName = "Realized", ForRecords = true,
                ForPeriodicRecord = false
            },
            new RecordStatus()
            {
                Id = Guid.NewGuid(), StatusCode = 3, RecordStatusName = "Active", ForRecords = false,
                ForPeriodicRecord = true
            },
            new RecordStatus()
            {
                Id = Guid.NewGuid(), StatusCode = 4, RecordStatusName = "Suspended", ForRecords = false,
                ForPeriodicRecord = true
            },
            new RecordStatus()
            {
                Id = Guid.NewGuid(), StatusCode = 5, RecordStatusName = "Pending", ForRecords = false,
                ForPeriodicRecord = true
            }
        ];
    }
}