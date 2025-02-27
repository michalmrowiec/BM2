using BM2.Shared.DTOs;
using BM2.Shared.SystemCodes;
using MudBlazor;

namespace BM2.Client.Services.DisplayServices;

public static class IconService
{
    public static string GetIconForRecordStatus(this RecordStatusDTO recordStatus)
    {
        return recordStatus.SystemCode switch
        {
            var code when code == StatusSystemCode.Planned => Icons.Material.Filled.PlaylistAddCheckCircle,
            var code when code == StatusSystemCode.Realized => Icons.Material.Filled.CheckCircle,
            var code when code == StatusSystemCode.Active => Icons.Material.Filled.PlayCircle,
            var code when code == StatusSystemCode.Suspended => Icons.Material.Filled.StopCircle,
            var code when code == StatusSystemCode.Pending => Icons.Material.Filled.AccessTimeFilled,
            _ => Icons.Material.Filled.CheckCircle
        };
    }
}