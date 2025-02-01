using BM2.Domain.Entities;
using BM2.Infrastructure.Seeders;
using Microsoft.EntityFrameworkCore;

namespace BM2.Infrastructure.Services;

public static class SeederService
{
    public static async Task SeedDatabaseAsync(this BM2DbContext context)
    {
        await RecordStatusSeeder.SeedAsync(context);
        await CurrencySeeder.SeedAsync(context);
    }
}