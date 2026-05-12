using BM2.Domain.Entities.UserRecords;
using BM2.Shared.Requests.Commands.Record;
using BM2.Shared.SystemCodes;
using Hangfire;
using MediatR;

namespace BM2.Application.Services;

public interface IPeriodicJobManager
{
    void ScheduleNextExecution(PeriodicRecordDefinition definition);
    void RemoveScheduledJob(PeriodicRecordDefinition definition);
}

public class PeriodicJobManager(IBackgroundJobClient _backgroundJobClient, IPeriodicRecordScheduler _scheduler) : IPeriodicJobManager
{
    public void ScheduleNextExecution(PeriodicRecordDefinition definition)
    {
        // 1. Usuń stary job jeśli istnieje
        RemoveScheduledJob(definition);

        if (definition.PeriodicRecordStatus?.SystemCode != StatusSystemCode.Active)
            return;

        // 2. Oblicz nową datę (wykorzystaj logikę z Twojego obecnego handlera)
        var nextDate = _scheduler.CalculateNextDate(definition.StartDate, DateTime.UtcNow, definition.Periodicity);

        // 3. Zaplanuj
        var jobId = _backgroundJobClient.Schedule<IMediator>(
            m => m.Send(new ExecutePeriodicRecordDefinitionCommand(definition.Id), CancellationToken.None),
            nextDate - DateTime.UtcNow);

        // 4. Zaktualizuj encję
        definition.NextExecutionAt = nextDate;
        definition.HangfireJobId = jobId;
    }

    public void RemoveScheduledJob(PeriodicRecordDefinition definition)
    {
        if (!string.IsNullOrEmpty(definition.HangfireJobId))
        {
            _backgroundJobClient.Delete(definition.HangfireJobId);
            definition.HangfireJobId = null;
            definition.NextExecutionAt = null;
        }
    }
}
