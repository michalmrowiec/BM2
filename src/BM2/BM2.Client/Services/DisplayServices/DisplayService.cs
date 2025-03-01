namespace BM2.Client.Services.DisplayServices;

internal static class DisplayService
{
    public static string DateTimeUtcToLocal(this DateTime dateTime)
    {
        return dateTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
    }
}