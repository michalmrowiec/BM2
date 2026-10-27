using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BM2.Client.Localization;
using Microsoft.AspNetCore.Components;

namespace BM2.Client.Services;

/// <summary>Calls the BM2 API and turns failures into <see cref="ApiProblem"/>; 401 sends the user to log in, 429 shows a toast.</summary>
public sealed class ApiClient(HttpClient http, NavigationManager navigation, ToastService toasts)
{
    public Task<ApiResult<T>> GetAsync<T>(string url) => SendAsync<T>(() => http.GetAsync(url));

    public Task<ApiResult<T>> PostAsync<T>(string url, object body) => SendAsync<T>(() => http.PostAsJsonAsync(url, body));

    public Task<ApiResult> PostAsync(string url, object? body = null) =>
        SendAsync(() => body is null ? http.PostAsync(url, null) : http.PostAsJsonAsync(url, body));

    public Task<ApiResult> PutAsync(string url, object body) => SendAsync(() => http.PutAsJsonAsync(url, body));

    public Task<ApiResult> DeleteAsync(string url) => SendAsync(() => http.DeleteAsync(url));

    private async Task<ApiResult<T>> SendAsync<T>(Func<Task<HttpResponseMessage>> send)
    {
        var (response, problem) = await TrySendAsync(send);
        if (response is null)
        {
            return new ApiResult<T>(default, problem);
        }

        using (response)
        {
            var value = response.StatusCode == HttpStatusCode.NoContent
                ? default
                : await response.Content.ReadFromJsonAsync<T>();
            return new ApiResult<T>(value, null);
        }
    }

    private async Task<ApiResult> SendAsync(Func<Task<HttpResponseMessage>> send)
    {
        var (response, problem) = await TrySendAsync(send);
        response?.Dispose();
        return new ApiResult(problem);
    }

    private async Task<(HttpResponseMessage? Response, ApiProblem? Problem)> TrySendAsync(Func<Task<HttpResponseMessage>> send)
    {
        HttpResponseMessage response;
        try
        {
            response = await send();
        }
        catch (HttpRequestException)
        {
            return (null, ApiProblem.Network);
        }

        if (response.IsSuccessStatusCode)
        {
            return (response, null);
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                navigation.NavigateTo($"Account/Login?returnUrl={Uri.EscapeDataString(navigation.Uri)}", forceLoad: true);
            }

            var problem = await ReadProblemAsync(response);
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                toasts.Show(problem.Detail ?? Strings.Tr("Too many requests. Wait a minute and try again."), ToastKind.Error);
            }

            return (null, problem);
        }
    }

    private static async Task<ApiProblem> ReadProblemAsync(HttpResponseMessage response)
    {
        var status = (int)response.StatusCode;
        var mediaType = response.Content.Headers.ContentType?.MediaType;
        if (mediaType is null || !mediaType.Contains("json", StringComparison.OrdinalIgnoreCase))
        {
            return new ApiProblem(status, null, new Dictionary<string, string[]>());
        }

        try
        {
            var payload = await response.Content.ReadFromJsonAsync<ProblemPayload>();
            var errors = payload?.Errors?.ToDictionary(e => e.Key, e => e.Value.Select(Strings.TrMessage).ToArray());
            return new ApiProblem(status, payload?.Detail is { } detail ? Strings.TrMessage(detail) : null,
                errors ?? new Dictionary<string, string[]>());
        }
        catch (JsonException)
        {
            return new ApiProblem(status, null, new Dictionary<string, string[]>());
        }
    }

    private sealed record ProblemPayload(string? Detail, Dictionary<string, string[]>? Errors);
}
