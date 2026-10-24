using BM2.Contracts.Wallets;
using BM2.Server.Features.Wallets;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BM2.Server.Features;

public static class FeatureServices
{
    public static IServiceCollection AddBm2Features(this IServiceCollection services)
    {
        services.AddScoped<CurrentWallet>();
        services.AddValidatorsFromAssemblyContaining<CreateWalletRequestValidator>(ServiceLifetime.Singleton);
        services.TryAddSingleton(TimeProvider.System);
        return services;
    }
}
