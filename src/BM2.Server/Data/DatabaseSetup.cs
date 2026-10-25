using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace BM2.Server.Data;

public static class DatabaseSetup
{
    // Production runs on SQL Server 2012 (Webio).
    public const int CompatibilityLevel = 110;

    // ALTER DATABASE does not accept parameters, so the level is a literal.
    private const string SetCompatibilityLevelSql = "ALTER DATABASE CURRENT SET COMPATIBILITY_LEVEL = 110";

    public static async Task PrepareAsync(AppDbContext db, CancellationToken ct = default)
    {
        var creator = db.GetService<IRelationalDatabaseCreator>();
        if (!await creator.ExistsAsync(ct))
        {
            await creator.CreateAsync(ct);
        }

        if (await GetCompatibilityLevelAsync(db, ct) != CompatibilityLevel)
        {
            await db.Database.ExecuteSqlRawAsync(SetCompatibilityLevelSql, ct);
        }

        await db.Database.MigrateAsync(ct);
    }

    public static Task<int> GetCompatibilityLevelAsync(AppDbContext db, CancellationToken ct = default) =>
        db.Database
            .SqlQuery<int>($"SELECT CAST(compatibility_level AS int) AS [Value] FROM sys.databases WHERE name = DB_NAME()")
            .SingleAsync(ct);
}
