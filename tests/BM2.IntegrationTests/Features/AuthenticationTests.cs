using System.Net;
using System.Text.RegularExpressions;
using BM2.IntegrationTests.Infrastructure;
using BM2.Server.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BM2.IntegrationTests.Features;

public partial class AuthenticationTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public void Identity_options_match_security_policy()
    {
        var options = Factory.Services.GetRequiredService<IOptions<IdentityOptions>>().Value;

        options.Password.RequiredLength.ShouldBe(12);
        options.Password.RequireDigit.ShouldBeFalse();
        options.Password.RequireLowercase.ShouldBeFalse();
        options.Password.RequireUppercase.ShouldBeFalse();
        options.Password.RequireNonAlphanumeric.ShouldBeFalse();
        options.Lockout.MaxFailedAccessAttempts.ShouldBe(5);
        options.Lockout.DefaultLockoutTimeSpan.ShouldBe(TimeSpan.FromMinutes(15));
    }

    [Fact]
    public void Application_cookie_is_long_lived_and_secure()
    {
        var cookie = Factory.Services.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(IdentityConstants.ApplicationScheme);

        cookie.ExpireTimeSpan.ShouldBe(TimeSpan.FromDays(30));
        cookie.SlidingExpiration.ShouldBeTrue();
        cookie.Cookie.HttpOnly.ShouldBeTrue();
        cookie.Cookie.SecurePolicy.ShouldBe(CookieSecurePolicy.Always);
        cookie.Cookie.SameSite.ShouldBe(SameSiteMode.Lax);
    }

    [Fact]
    public async Task Registration_page_is_not_available_outside_development()
    {
        using var client = Factory.CreateAnonymousClient();

        var response = await client.GetAsync("/Account/Register", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Login_page_has_no_register_link_outside_development()
    {
        using var client = Factory.CreateAnonymousClient();

        var html = await client.GetStringAsync("/Account/Login", TestContext.Current.CancellationToken);

        html.ShouldNotContain("Account/Register");
    }

    [Fact]
    public async Task Five_failed_logins_lock_the_account()
    {
        var user = await CreateUserAsync("lock@example.com");
        using var client = Factory.CreateAnonymousClient();

        for (var attempt = 0; attempt < 5; attempt++)
        {
            await PostLoginAsync(client, user.Email!, "wrong-password-123");
        }

        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var stored = await userManager.FindByIdAsync(user.Id.ToString());
        (await userManager.IsLockedOutAsync(stored!)).ShouldBeTrue();
    }

    private static async Task PostLoginAsync(HttpClient client, string email, string password)
    {
        var ct = TestContext.Current.CancellationToken;
        var page = await client.GetStringAsync("/Account/Login", ct);
        var token = AntiforgeryToken().Match(page).Groups[1].Value;
        token.ShouldNotBeNullOrEmpty();

        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["_handler"] = "login",
            ["__RequestVerificationToken"] = token,
            ["Input.Email"] = email,
            ["Input.Password"] = password,
            ["Input.RememberMe"] = "false",
        });
        using var response = await client.PostAsync("/Account/Login", form, ct);
        ((int)response.StatusCode).ShouldBeLessThan(500);
    }

    [GeneratedRegex("__RequestVerificationToken[^>]*value=\"([^\"]+)\"")]
    private static partial Regex AntiforgeryToken();
}
