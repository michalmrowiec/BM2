using BM2.Server.Data;
using BM2.Server.Features.Invitations;

namespace BM2.Server.Infrastructure;

/// <summary>Outside Development, registration needs a valid invitation link; the confirmation page stays closed.</summary>
public static class RegistrationGate
{
    public static WebApplication UseRegistrationOnlyInDevelopment(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            return app;
        }

        app.Use(async (context, next) =>
        {
            var path = context.Request.Path;
            var closed = path.StartsWithSegments("/Account/RegisterConfirmation")
                || (path.StartsWithSegments("/Account/Register") && !await HasUsableInvitationAsync(context));
            if (closed)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            await next(context);
        });

        return app;
    }

    public static async Task<bool> HasUsableInvitationAsync(HttpContext context)
    {
        var token = context.Request.Query["invite"].ToString();
        if (token.Length == 0)
        {
            return false;
        }

        var db = context.RequestServices.GetRequiredService<AppDbContext>();
        var time = context.RequestServices.GetRequiredService<TimeProvider>();
        var invitation = await InvitationLookup.FindAsync(db, token, context.RequestAborted);
        return invitation?.IsPending(time.GetUtcNow()) == true;
    }
}
