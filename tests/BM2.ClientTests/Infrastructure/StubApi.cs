using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace BM2.ClientTests.Infrastructure;

/// <summary>Fake API: answers by method and path (with query) and records every request as "METHOD path".</summary>
public sealed class StubApi : HttpMessageHandler
{
    private readonly List<(HttpMethod Method, string Path, Func<HttpRequestMessage, HttpResponseMessage> Respond)> _routes = [];

    public List<string> Requests { get; } = [];

    public List<(string Request, string? Body)> Bodies { get; } = [];

    public StubApi On(HttpMethod method, string path, Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        _routes.Insert(0, (method, path, respond));
        return this;
    }

    public StubApi OnGet<T>(string path, T body) => On(HttpMethod.Get, path, _ => Json(body));

    public HttpClient CreateClient() => new(this) { BaseAddress = new Uri("http://localhost/") };

    public static HttpResponseMessage Json<T>(T body, HttpStatusCode status = HttpStatusCode.OK) =>
        new(status) { Content = JsonContent.Create(body) };

    public static HttpResponseMessage Problem(HttpStatusCode status, IDictionary<string, string[]>? errors = null) =>
        new(status)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new { status = (int)status, errors }),
                Encoding.UTF8,
                "application/problem+json"),
        };

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var path = request.RequestUri!.PathAndQuery.TrimStart('/');
        var key = $"{request.Method} {path}";
        Requests.Add(key);
        Bodies.Add((key, request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken)));

        var route = _routes.FirstOrDefault(r => r.Method == request.Method && r.Path == path);
        return route.Respond is null ? new HttpResponseMessage(HttpStatusCode.NotFound) : route.Respond(request);
    }
}
