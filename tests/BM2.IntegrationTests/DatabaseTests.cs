using BM2.IntegrationTests.Infrastructure;
using BM2.Server.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BM2.IntegrationTests;

public class DatabaseTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Test_database_runs_in_sql_server_2012_compatibility_level()
    {
        await using var db = Fixture.CreateDbContext();

        (await DatabaseSetup.GetCompatibilityLevelAsync(db, TestContext.Current.CancellationToken)).ShouldBe(110);
    }

    [Fact]
    public async Task Data_protection_keys_survive_app_restart()
    {
        string payload;
        await using (var first = Fixture.CreateFactory())
        {
            payload = first.Services.GetRequiredService<IDataProtectionProvider>()
                .CreateProtector("restart-test").Protect("hello");
        }

        await using var second = Fixture.CreateFactory();
        var restored = second.Services.GetRequiredService<IDataProtectionProvider>()
            .CreateProtector("restart-test").Unprotect(payload);

        restored.ShouldBe("hello");
        await using var db = Fixture.CreateDbContext();
        (await db.DataProtectionKeys.CountAsync(TestContext.Current.CancellationToken)).ShouldBeGreaterThan(0);
    }
}
