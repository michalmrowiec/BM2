using BM2.Client.State;
using Microsoft.Extensions.DependencyInjection;

namespace BM2.Client.Services;

public static class ClientServices
{
    /// <summary>Registered on the server too, so prerendered components can be created (they call the API only once interactive).</summary>
    public static IServiceCollection AddBm2Client(this IServiceCollection services)
    {
        services.AddScoped<ApiClient>();
        services.AddScoped<BrowserStorage>();
        services.AddScoped<ToastService>();
        services.AddScoped<WalletState>();
        services.AddScoped<StatsState>();
        services.AddScoped<AppNavigation>();
        return services;
    }
}
