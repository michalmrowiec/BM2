using System.Net;
using BM2.Application.Exceptions;
using BM2.Middleware.Utils;

namespace BM2.Middleware
{
    public class ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger) : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (DomainExceptions.UnauthenticatedException ex)
            {
                logger.LogWarning(ex, "Unauthenticated: {Message}", ex.Message);
                await context.HandleExceptionAsync(HttpStatusCode.Unauthorized, "Unauthenticated");
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogWarning(ex, "Unauthorized: {Message}", ex.Message);
                await context.HandleExceptionAsync(HttpStatusCode.Forbidden, "Unauthorized access");
            }
            catch (DomainExceptions.NotFoundException ex)
            {
                logger.LogWarning(ex, "Not found: {Message}", ex.Message);
                await context.HandleExceptionAsync(HttpStatusCode.NotFound, "Not found");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error: {Message}", ex.Message);
                await context.HandleExceptionAsync(HttpStatusCode.InternalServerError, "An unexpected error occurred");
            }
        }
    }
}