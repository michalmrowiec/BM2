using BM2.Server.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace BM2.IntegrationTests.Infrastructure;

[Collection(IntegrationCollection.Name)]
public abstract class IntegrationTest(IntegrationFixture fixture) : IAsyncLifetime
{
    protected const string DefaultPassword = "correct-horse-battery-staple";

    protected IntegrationFixture Fixture { get; } = fixture;

    protected BM2AppFactory Factory => Fixture.Factory;

    public async ValueTask InitializeAsync() => await Fixture.ResetDatabaseAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    protected async Task<ApplicationUser> CreateUserAsync(string email, string password = DefaultPassword)
    {
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, password);
        result.Succeeded.ShouldBeTrue(string.Join("; ", result.Errors.Select(e => e.Description)));
        return user;
    }
}
