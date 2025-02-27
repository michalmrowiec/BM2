using BM2.Shared.DTOs;
using BM2.Shared.SystemCodes;
using MudBlazor;

namespace BM2.Client.Services.DisplayServices;

public static class ColorService
{
    public static Color GetColorForRecordStatus(this RecordStatusDTO recordStatus)
    {
        return recordStatus.SystemCode switch
        {
            var code when code == StatusSystemCode.Planned => Color.Info,
            var code when code == StatusSystemCode.Realized => Color.Success,
            var code when code == StatusSystemCode.Active => Color.Success,
            var code when code == StatusSystemCode.Suspended => Color.Error,
            var code when code == StatusSystemCode.Pending => Color.Warning,
            _ => Color.Default
        };
    }
}