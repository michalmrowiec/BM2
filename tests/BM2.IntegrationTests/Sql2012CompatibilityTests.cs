using BM2.IntegrationTests.Infrastructure;
using BM2.Server.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BM2.IntegrationTests;

// The local/CI engines are newer than production (SQL Server 2012), so a guard rejects newer T-SQL in tests.
public class Sql2012CompatibilityTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Guard_rejects_functions_missing_in_sql_server_2012()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var error = await Should.ThrowAsync<InvalidOperationException>(() =>
            db.Database.SqlQueryRaw<string>("SELECT JSON_VALUE(N'{{\"a\":\"1\"}}', '$.a') AS [Value]")
                .ToListAsync(TestContext.Current.CancellationToken));

        error.Message.ShouldContain("JSON_VALUE");
    }

    [Fact]
    public async Task Passkey_lifecycle_uses_only_sql_server_2012_features()
    {
        var user = await CreateUserAsync("passkey@example.com");
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var stored = (await userManager.FindByIdAsync(user.Id.ToString()))!;
        byte[] credentialId = [1, 2, 3, 4];

        var passkey = new UserPasskeyInfo(credentialId, [5, 6, 7], DateTimeOffset.UtcNow, signCount: 0,
            transports: ["internal"], isUserVerified: true, isBackupEligible: true, isBackedUp: false,
            attestationObject: [8], clientDataJson: [9]) { Name = "Phone" };
        (await userManager.AddOrUpdatePasskeyAsync(stored, passkey)).Succeeded.ShouldBeTrue();

        // Rename and sign-in counter update: the partial-update path EF could translate to JSON_MODIFY.
        var saved = (await userManager.GetPasskeyAsync(stored, credentialId))!;
        saved.Name = "Renamed";
        saved.SignCount = 1;
        (await userManager.AddOrUpdatePasskeyAsync(stored, saved)).Succeeded.ShouldBeTrue();

        var reloaded = (await userManager.GetPasskeysAsync(stored)).ShouldHaveSingleItem();
        reloaded.Name.ShouldBe("Renamed");
        reloaded.SignCount.ShouldBe(1u);
        (await userManager.RemovePasskeyAsync(stored, credentialId)).Succeeded.ShouldBeTrue();
    }
}
