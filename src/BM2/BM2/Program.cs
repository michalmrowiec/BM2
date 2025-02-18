using BM2.Application;
using BM2.Client.Services.API;
using BM2.Client.Services.Auth;
using BM2.Client.Services.LocalStorage;
using BM2.Client.Services.Notification;
using BM2.Components;
using BM2.Controllers.Utils;
using BM2.Infrastructure;
using BM2.Infrastructure.Services;
using BM2.Middleware;
using BM2.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.OpenApi.Models;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
//builder.Services.AddAuthorizationCore();
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<IAlertService, AlertService>();
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddTransient<IApiClient, ApiClient>();
builder.Services.AddHttpClient();

builder.Services.AddMudServices();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(cfg =>
{
    cfg.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please insert token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "bearer"
        });

    cfg.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});
builder.Services.AddScoped<ErrorHandlingMiddleware>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

using var scope = app.Services.CreateScope();

var dbContext = scope.ServiceProvider
    .GetRequiredService<BM2DbContext>();

dbContext.UpdateDatabase(scope.ServiceProvider.GetRequiredService<ILogger<Program>>());

await dbContext.SeedDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MB2"));
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseAuthorization();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapControllers();

app.MapBlazorHub();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BM2.Client._Imports).Assembly);

app.UseCors("AllowAll");

app.Run();