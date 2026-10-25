namespace BM2.Server.Infrastructure;

public static class ErrorHandling
{
    /// <summary>API requests get ProblemDetails JSON; pages get the Razor error and not-found pages.</summary>
    public static WebApplication UseBm2ErrorHandling(this WebApplication app)
    {
        app.UseWhen(context => context.Request.IsApiRequest(), api =>
        {
            api.UseExceptionHandler();
            api.UseStatusCodePages();
        });

        app.UseWhen(context => !context.Request.IsApiRequest(), pages =>
        {
            if (!app.Environment.IsDevelopment())
            {
                pages.UseExceptionHandler("/Error", createScopeForErrors: true);
            }

            pages.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        });

        return app;
    }
}
