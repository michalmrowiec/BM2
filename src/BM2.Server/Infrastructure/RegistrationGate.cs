namespace BM2.Server.Infrastructure;

/// <summary>Self-registration stays closed outside Development until invitations exist.</summary>
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
            if (IsRegistrationPath(context.Request.Path))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            await next(context);
        });

        return app;
    }

    private static bool IsRegistrationPath(PathString path) =>
        path.StartsWithSegments("/Account/Register") || path.StartsWithSegments("/Account/RegisterConfirmation");
}
