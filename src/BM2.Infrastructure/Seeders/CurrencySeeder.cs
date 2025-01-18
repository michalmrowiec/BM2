using BM2.Domain.Entities;

namespace BM2.Infrastructure.Seeders;

internal static class CurrencySeeder
{
    internal static async Task Seed(this BM2DbContext context)
    {
        if (!context.Currencies.Any())
        {
            await context.Currencies.AddRangeAsync(GetCurrencies());

            await context.SaveChangesAsync();
        }
    }

    private static List<Currency> GetCurrencies()
    {
        return [];
    }
}