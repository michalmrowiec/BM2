using BM2.Client.Services;
using BM2.Server.Components;
using BM2.Server.Components.Account;
using BM2.Server.Data;
using BM2.Server.Features;
using BM2.Server.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

// Resolved lazily so integration tests can override the connection string.
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    var connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString("BM2")
        ?? throw new InvalidOperationException("Connection string 'BM2' not found.");
    options.UseSqlServer(connectionString, sql => sql.UseCompatibilityLevel(DatabaseSetup.CompatibilityLevel));
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddBm2Authentication();
builder.Services.AddBm2RateLimiting(builder.Configuration);

// Client services for prerendering; components call the API only once WebAssembly runs.
builder.Services.AddBm2Client();
builder.Services.AddScoped(_ => new HttpClient());
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
builder.Services.AddBm2Features();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

// Development and tests only; production applies migrations with a script at deploy time.
if (app.Configuration.GetValue<bool>("Database:PrepareOnStartup"))
{
    await using var scope = app.Services.CreateAsyncScope();
    await DatabaseSetup.PrepareAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>());
}

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseHsts();
}
app.UseBm2ErrorHandling();
app.UseHttpsRedirection();
app.UseBm2Localization();

// Explicit, so auth runs inside error handling (otherwise WebApplication adds it at the very start).
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseAntiforgery();
app.UseRegistrationOnlyInDevelopment();

app.MapStaticAssets();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapBm2Api();
app.MapCultureEndpoint();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BM2.Client._Imports).Assembly);

// Endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

await app.RunAsync();
