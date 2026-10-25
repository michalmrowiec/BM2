namespace BM2.Server.Infrastructure;

public static class ApiPaths
{
    public const string Prefix = "/api";

    public static bool IsApiRequest(this HttpRequest request) => request.Path.StartsWithSegments(Prefix);
}
