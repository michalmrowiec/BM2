using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace BM2.Application;

public static class ApplicationInstallation
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => { cfg.RegisterServicesFromAssembly(typeof(ApplicationInstallation).Assembly); });

        services.AddAutoMapper(Assembly.GetExecutingAssembly());
    }
}