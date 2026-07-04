using BM2.Shared.DTOs;

namespace BM2.Client.Services.States;

public interface IRecordEventService
{
    event Func<Task>? OnRecordsChanged;
    Task NotifyRecordsChanged();
}

public class RecordEventService : IRecordEventService
{
    public event Func<Task>? OnRecordsChanged;

    public async Task NotifyRecordsChanged()
    {
        if (OnRecordsChanged != null)
        {
            await OnRecordsChanged.Invoke();
        }
    }
}