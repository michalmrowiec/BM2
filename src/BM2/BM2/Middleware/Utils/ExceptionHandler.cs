using System.Net;
using Newtonsoft.Json;

namespace BM2.Middleware.Utils;

internal static class ExceptionHandler
{
    internal static async Task HandleExceptionAsync(this HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new { statusCode = (int)statusCode, message };

        await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
    }
}