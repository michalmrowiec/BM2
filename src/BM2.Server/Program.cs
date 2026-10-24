using BM2.Server.Components;
using BM2.Server.Components.Account;
using BM2.Server.Data;
using BM2.Server.Infrastructure;
using Microsoft.EntityFrameworkCore;

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
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();
app.UseRegistrationOnlyInDevelopment();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BM2.Client._Imports).Assembly);

// Endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

await app.RunAsync();
