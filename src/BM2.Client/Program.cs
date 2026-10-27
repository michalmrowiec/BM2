using BM2.Client.Localization;
using BM2.Client.Services;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

// Same origin as the server, so the auth cookie is sent automatically.
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddBm2Client();

var host = builder.Build();

// The server renders <html lang> from the culture cookie; use it before the first render so nothing flashes in English.
var language = await host.Services.GetRequiredService<IJSRuntime>()
    .InvokeAsync<string?>("document.documentElement.getAttribute", "lang");
Strings.Apply(Strings.CultureFor(language));

await host.RunAsync();
