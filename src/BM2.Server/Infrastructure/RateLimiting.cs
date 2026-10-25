using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace BM2.Server.Infrastructure;

/// <summary>
/// Per-minute limits: account form posts per IP (login, register, password reset), API writes per user and the
/// export policy per user. RateLimiting:Enabled=false turns them off (integration tests).
/// </summary>
public static class RateLimiting
{
    public const string ExportPolicy = "export";
    private const string TooMany = "Too many requests. Wait a minute and try again.";

    public static IServiceCollection AddBm2RateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var enabled = configuration.GetValue("RateLimiting:Enabled", true);
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = RejectAsync;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var request = context.Request;
                if (!enabled || HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method))
                {
                    return RateLimitPartition.GetNoLimiter("none");
                }

                if (request.Path.StartsWithSegments("/Account"))
                {
                    return PerMinute("account:" + context.Connection.RemoteIpAddress, 20);
                }

                return request.Path.StartsWithSegments(ApiPaths.Prefix)
                    ? PerMinute("write:" + UserKey(context), 120)
                    : RateLimitPartition.GetNoLimiter("none");
            });
            options.AddPolicy(ExportPolicy, context =>
                enabled ? PerMinute("export:" + UserKey(context), 5) : RateLimitPartition.GetNoLimiter("none"));
        });
        return services;
    }

    private static RateLimitPartition<string> PerMinute(string key, int permits) =>
        RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permits,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
        });

    private static string UserKey(HttpContext context) =>
        context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "ip:" + context.Connection.RemoteIpAddress;

    private static async ValueTask RejectAsync(OnRejectedContext rejected, CancellationToken ct)
    {
        var context = rejected.HttpContext;
        if (rejected.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        if (context.Request.Path.StartsWithSegments(ApiPaths.Prefix))
        {
            await context.Response.WriteAsJsonAsync(
                new { title = "Too many requests", status = 429, detail = TooMany }, (System.Text.Json.JsonSerializerOptions?)null,
                "application/problem+json", ct);
        }
        else
        {
            await context.Response.WriteAsync(TooMany, ct);
        }
    }
}
