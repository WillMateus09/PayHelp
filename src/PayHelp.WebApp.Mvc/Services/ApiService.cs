using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace PayHelp.WebApp.Mvc.Services;

public class ApiService
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiService(HttpClient http, IHttpContextAccessor accessor)
    {
        _http = http;
        _httpContextAccessor = accessor;
        if (_http.BaseAddress == null)
            _http.BaseAddress = new Uri("https://localhost:7236/api/");
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        AttachBearerIfPresent();
        var response = await SendWithRedirectAsync(new HttpRequestMessage(HttpMethod.Get, endpoint));
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Sessão expirada");
        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            throw new UnauthorizedAccessException("Acesso negado");
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"API error {(int)response.StatusCode} {response.StatusCode}: {err}");
        }
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest body)
    {
        AttachBearerIfPresent();
        var req = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = JsonContent.Create(body)
        };
        var resp = await SendWithRedirectAsync(req);
        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Sessão expirada");
        if (resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
            throw new UnauthorizedAccessException("Acesso negado");
        if (!resp.IsSuccessStatusCode)
        {
            var err = await resp.Content.ReadAsStringAsync();
            throw new HttpRequestException($"API error {(int)resp.StatusCode} {resp.StatusCode}: {err}");
        }
        return await resp.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
    }

    public async Task DeleteAsync(string endpoint)
    {
        AttachBearerIfPresent();
        using var req = new HttpRequestMessage(HttpMethod.Delete, endpoint);
        var resp = await SendWithRedirectAsync(req);
        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Sessão expirada");
        if (resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
            throw new UnauthorizedAccessException("Acesso negado");
        if (!resp.IsSuccessStatusCode)
        {
            var err = await resp.Content.ReadAsStringAsync();
            throw new HttpRequestException($"API error {(int)resp.StatusCode} {resp.StatusCode}: {err}");
        }
    }

    private void AttachBearerIfPresent()
    {
        var ctx = _httpContextAccessor.HttpContext;
        var token = ctx?.Session?.GetString("ApiJwt");
        if (!string.IsNullOrWhiteSpace(token))
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _http.DefaultRequestHeaders.Authorization = null;
        }
    }


    private async Task<HttpResponseMessage> SendWithRedirectAsync(HttpRequestMessage initial)
    {
        var response = await _http.SendAsync(initial);
        if ((int)response.StatusCode == 307 || (int)response.StatusCode == 308)
        {
            if (response.Headers.Location != null)
            {
                var target = response.Headers.Location.IsAbsoluteUri
                    ? response.Headers.Location
                    : new Uri(_http.BaseAddress!, response.Headers.Location);
                using var retry = new HttpRequestMessage(initial.Method, target)
                {
                    Content = initial.Content
                };
                response.Dispose();
                response = await _http.SendAsync(retry);
            }
        }
        return response;
    }
}
