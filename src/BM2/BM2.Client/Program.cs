using BM2.Client.Services;
using BM2.Client.Services.API;
using BM2.Client.Services.Auth;
using BM2.Client.Services.LocalStorage;
using BM2.Client.Services.Notification;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(@"https://localhost:7177") });

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddAuthorizationCore();
builder.Services.AddSingleton<IAlertService, AlertService>();
builder.Services.AddSingleton<IWalletSelectionState, WalletSelectionState>();
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddTransient<IApiClient, ApiClient>();

builder.Services.AddMudServices();
builder.Services.AddMudBlazorSnackbar(config =>
{
    config.PositionClass = Defaults.Classes.Position.BottomEnd;
});

await builder.Build().RunAsync();
