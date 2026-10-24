using BM2.Server.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Respawn;
using Respawn.Graph;

namespace BM2.IntegrationTests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class IntegrationCollection : ICollectionFixture<IntegrationFixture>
{
    public const string Name = "Integration";
}

/// <summary>One throwaway SQL Server database per test run, in SQL Server 2012 compatibility mode.</summary>
public sealed class IntegrationFixture : IAsyncLifetime
{
    private const string DefaultServer = @"Server=localhost\SQLEXPRESS;Integrated Security=True;TrustServerCertificate=True";

    private Respawner? _respawner;
    private BM2AppFactory? _factory;

    public string ConnectionString { get; } = new SqlConnectionStringBuilder(
        Environment.GetEnvironmentVariable("BM2_TEST_SQL") ?? DefaultServer)
    {
        InitialCatalog = $"BM2_Test_{Guid.NewGuid():N}",
    }.ConnectionString;

    public BM2AppFactory Factory => _factory ?? throw new InvalidOperationException("Fixture not initialized.");

    public BM2AppFactory CreateFactory() => new(ConnectionString);

    public async ValueTask InitializeAsync()
    {
        _factory = CreateFactory();
        await using (var db = CreateDbContext())
        {
            await DatabaseSetup.PrepareAsync(db);
        }

        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            TablesToIgnore = [new Table("__EFMigrationsHistory"), new Table("DataProtectionKeys")],
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        await _respawner!.ResetAsync(connection);
    }

    // The app's service provider is required: the Identity model (schema v3, passkeys) depends on IdentityOptions.
    public AppDbContext CreateDbContext() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer(ConnectionString, sql => sql.UseCompatibilityLevel(DatabaseSetup.CompatibilityLevel))
        .UseApplicationServiceProvider(Factory.Services)
        .Options);

    public async ValueTask DisposeAsync()
    {
        if (_factory is null)
        {
            return;
        }

        SqlConnection.ClearAllPools();
        await using (var db = CreateDbContext())
        {
            await db.Database.EnsureDeletedAsync();
        }

        await _factory.DisposeAsync();
    }
}
