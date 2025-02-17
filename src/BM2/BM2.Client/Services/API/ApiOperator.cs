﻿using System.Net.Http.Headers;
using System.Text;
using BM2.Client.Services.Auth;
using Newtonsoft.Json;

namespace BM2.Client.Services.API;

public interface IApiOperator
{
    Task<HttpResponseMessage> Get(string uri, Dictionary<string, string>? queryParams = null);
    Task<HttpResponseMessage> Create(string uri, object item);

}

public class ApiOperator(
    HttpClient httpClient,
    IAuthService authService) : IApiOperator
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IAuthService _authService = authService;
    
    protected async Task<HttpResponseMessage> BaseRequestWithAuth(
        HttpMethod httpMethod,
        string uri,
        object? item = null,
        Dictionary<string, string>? queryParams = null)
    {
        if (queryParams != null && queryParams.Count > 0)
        {
            var query = string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
            uri = $"{uri}?{query}";
        }

        using var request = new HttpRequestMessage(httpMethod, uri);

        if (item != null)
        {
            var postJson = new StringContent(JsonConvert.SerializeObject(item), Encoding.UTF8, "application/json");
            request.Content = postJson;
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _authService.GetJwtToken());
        var response = await _httpClient.SendAsync(request);

        return response;
    }
    
    public async Task<HttpResponseMessage> Get(string uri, Dictionary<string, string>? queryParams = null)
    {
        return await BaseRequestWithAuth(
            HttpMethod.Get,
            uri: uri,
            queryParams: queryParams);
    }
    
    public async Task<HttpResponseMessage> Create(string uri, object item)
    {
        return await BaseRequestWithAuth(
            HttpMethod.Post,
            uri: uri,
            item: item);
    }
}