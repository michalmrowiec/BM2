using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace BM2.IntegrationTests.Infrastructure;

public sealed class BM2AppFactory(string connectionString) : WebApplicationFactory<Program>
{
    private const string TestOrIdentityScheme = "TestOrIdentity";

    public HttpClient CreateAnonymousClient() => CreateClient(CreateClientOptions());

    public HttpClient CreateClientAs(Guid userId)
    {
        var client = CreateClient(CreateClientOptions());
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:BM2", connectionString);
        builder.UseSetting("Database:PrepareOnStartup", "true");
        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { })
                .AddPolicyScheme(TestOrIdentityScheme, null, options => options.ForwardDefaultSelector = context =>
                    context.Request.Headers.ContainsKey(TestAuthHandler.UserIdHeader)
                    || context.Request.Headers.ContainsKey(TestAuthHandler.ThrowHeader)
                        ? TestAuthHandler.SchemeName
                        : IdentityConstants.ApplicationScheme);

            services.Configure<AuthenticationOptions>(options =>
            {
                options.DefaultScheme = TestOrIdentityScheme;
                options.DefaultAuthenticateScheme = TestOrIdentityScheme;
                options.DefaultChallengeScheme = TestOrIdentityScheme;
            });
        });
    }

    private static WebApplicationFactoryClientOptions CreateClientOptions() => new()
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false,
    };
}
