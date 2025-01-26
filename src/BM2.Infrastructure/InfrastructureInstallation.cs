using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BM2.Infrastructure;

public static class InfrastructureInstallation
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BM2DbContext>(
            opt => opt.UseSqlServer(configuration.GetConnectionString("BM2DB")));
    }
}