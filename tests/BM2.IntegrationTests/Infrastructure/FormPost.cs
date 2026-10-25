using System.Text.RegularExpressions;

namespace BM2.IntegrationTests.Infrastructure;

/// <summary>Submits a server-rendered (SSR) form the way a browser does, with its antiforgery token.</summary>
public static partial class FormPost
{
    public static async Task<HttpResponseMessage> SubmitAsync(
        HttpClient client, string url, string handler, IReadOnlyDictionary<string, string> fields, CancellationToken ct)
    {
        var page = await client.GetStringAsync(url, ct);
        var token = AntiforgeryToken().Match(page).Groups[1].Value;
        token.ShouldNotBeNullOrEmpty();

        var values = new Dictionary<string, string>(fields)
        {
            ["_handler"] = handler,
            ["__RequestVerificationToken"] = token,
        };
        using var form = new FormUrlEncodedContent(values);
        return await client.PostAsync(url, form, ct);
    }

    [GeneratedRegex("__RequestVerificationToken[^>]*value=\"([^\"]+)\"")]
    private static partial Regex AntiforgeryToken();
}
