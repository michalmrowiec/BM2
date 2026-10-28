using BM2.Client.Localization;
using Microsoft.AspNetCore.Localization;

namespace BM2.Server.Infrastructure;

/// <summary>The language is a cookie (<c>.AspNetCore.Culture</c>); English when it's missing.</summary>
public static class Localization
{
    public static IApplicationBuilder UseBm2Localization(this IApplicationBuilder app) =>
        app.UseRequestLocalization(options =>
        {
            options.SetDefaultCulture(Strings.English)
                .AddSupportedCultures(Strings.Cultures)
                .AddSupportedUICultures(Strings.Cultures);
            options.RequestCultureProviders = [new CookieRequestCultureProvider()];
        });

    /// <summary>GET /culture/{language}?returnUrl=… saves the choice and reloads, so WebAssembly starts in it.</summary>
    public static void MapCultureEndpoint(this IEndpointRouteBuilder app) =>
        app.MapGet("/culture/{language}", (string language, string? returnUrl, HttpContext context) =>
        {
            var culture = Strings.CultureFor(language);
            context.Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    IsEssential = true,
                });
            return TypedResults.LocalRedirect(IsLocal(returnUrl) ? returnUrl! : "/");
        }).AllowAnonymous();

    private static bool IsLocal(string? url) =>
        url is ['/', ..] && !url.StartsWith("//", StringComparison.Ordinal) && !url.StartsWith("/\\", StringComparison.Ordinal);
}
