using System.Security.Claims;
using BM2.Contracts.Common;
using BM2.Server.Data;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Infrastructure;

/// <summary>
/// Shared read-only demo (Demo:Enabled). POST /Account/Demo signs in as the demo user, created with sample data on first
/// use. The user has no password or passkey; its role claim makes the server refuse every write it sends.
/// </summary>
public static partial class DemoAccount
{
    // Reserved .invalid domain: no real mailbox, and only the role claim (added here) makes a user the demo.
    public const string Email = "demo@bm2.invalid";

    // Paths a demo visitor may still post to: leaving the demo, logging in or registering a real account.
    private static readonly string[] AllowedPosts =
        ["/Account/Logout", "/Account/Login", "/Account/Register", "/Account/PasskeyRequestOptions", "/Account/Demo"];

    private static readonly SemaphoreSlim SeedLock = new(1, 1);

    public static bool IsEnabled(IConfiguration configuration) => configuration.GetValue<bool>("Demo:Enabled");

    public static WebApplication UseDemoReadOnly(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            var request = context.Request;
            var write = !HttpMethods.IsGet(request.Method) && !HttpMethods.IsHead(request.Method) && !HttpMethods.IsOptions(request.Method);
            if (write && context.User.IsInRole(Demo.Role) && !AllowedPosts.Any(path => request.Path.StartsWithSegments(path)))
            {
                if (request.IsApiRequest())
                {
                    await TypedResults.Problem(Demo.ReadOnlyMessage, statusCode: StatusCodes.Status403Forbidden).ExecuteAsync(context);
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync(Demo.ReadOnlyMessage);
                }

                return;
            }

            await next(context);
        });
        return app;
    }

    public static void MapDemoLogin(this WebApplication app) =>
        app.MapPost("/Account/Demo", async (
            HttpContext context,
            IConfiguration configuration,
            IAntiforgery antiforgery,
            UserManager<ApplicationUser> users,
            SignInManager<ApplicationUser> signIn,
            AppDbContext db,
            TimeProvider time,
            ILoggerFactory loggers) =>
        {
            if (!IsEnabled(configuration))
            {
                return Results.NotFound();
            }

            if (!await antiforgery.IsRequestValidAsync(context))
            {
                return Results.BadRequest();
            }

            var user = await EnsureUserAsync(users, db, time, context.RequestAborted);
            if (user is null)
            {
                LogEmailTaken(loggers.CreateLogger(nameof(DemoAccount)), Email);
                return Results.NotFound();
            }

            await signIn.SignInAsync(user, isPersistent: false);
            return Results.LocalRedirect("~/");
        });

    /// <summary>Creates the demo user and its wallet when missing; null when the email belongs to someone else.</summary>
    private static async Task<ApplicationUser?> EnsureUserAsync(
        UserManager<ApplicationUser> users, AppDbContext db, TimeProvider time, CancellationToken ct)
    {
        await SeedLock.WaitAsync(ct);
        try
        {
            var user = await users.FindByEmailAsync(Email);
            if (user is null)
            {
                user = new ApplicationUser { UserName = Email, Email = Email, EmailConfirmed = true };
                ThrowIfFailed(await users.CreateAsync(user));
                ThrowIfFailed(await users.AddClaimAsync(user, new Claim(ClaimTypes.Role, Demo.Role)));
            }
            else if (!(await users.GetClaimsAsync(user)).Any(c => c.Type == ClaimTypes.Role && c.Value == Demo.Role))
            {
                return null;
            }

            // Also repairs a demo user whose first seeding failed.
            if (!await db.WalletMembers.AnyAsync(m => m.UserId == user.Id, ct))
            {
                await DemoData.SeedAsync(db, user.Id, time, ct);
            }

            return user;
        }
        finally
        {
            SeedLock.Release();
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Demo email {Email} belongs to a user without the demo role.")]
    private static partial void LogEmailTaken(ILogger logger, string email);

    private static void ThrowIfFailed(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Creating the demo user failed: " + string.Join(", ", result.Errors.Select(e => e.Code)));
        }
    }
}
