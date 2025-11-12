using System.Net.Http.Json;
using PayHelp.Mobile.Maui.Models;

namespace PayHelp.Mobile.Maui.Services;

public class AuthService
{
    private readonly IHttpClientFactory _factory;

    public AuthService(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    public async Task<(LoginResponse? data, string? error)> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        try
        {
            var client = _factory.CreateClient("api");
            var res = await client.PostAsJsonAsync("auth/login", request, ct);
            if (res.IsSuccessStatusCode)
            {
                var dto = await res.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct);
                return (dto, null);
            }

            if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return (null, "E-mail ou senha incorretos.");
            }

            var msg = await res.Content.ReadAsStringAsync(ct);
            return (null, NormalizeApiError(msg));
        }
        catch (TaskCanceledException)
        {
            return (null, "Tempo esgotado ao comunicar com a API. Verifique se a API está em execução e o endereço/porta está correto em Configurações.");
        }
        catch (HttpRequestException ex)
        {
            return (null, $"Falha de conexão: {ex.Message}");
        }
    }

    public async Task<(bool ok, string? error)> RegisterSimplesAsync(RegisterRequest request, CancellationToken ct = default)
    {
        try
        {
            var client = _factory.CreateClient("api");
            var body = new
            {
                NumeroInscricao = request.NumeroInscricao,
                Nome = request.Nome,
                Email = request.Email,
                Senha = request.Senha
            };
            var res = await client.PostAsJsonAsync("auth/register/simples", body, ct);
            if (res.IsSuccessStatusCode) return (true, null);
            var msg = await res.Content.ReadAsStringAsync(ct);
            return (false, NormalizeApiError(msg));
        }
        catch (TaskCanceledException)
        {
            return (false, "Tempo esgotado ao comunicar com a API. Verifique se a API está em execução e o endereço/porta está correto em Configurações.");
        }
        catch (HttpRequestException ex)
        {
            return (false, $"Falha de conexão: {ex.Message}");
        }
    }

    public async Task<(bool ok, string? error)> RegisterSuporteAsync(RegisterRequest request, CancellationToken ct = default)
    {
        try
        {
            var client = _factory.CreateClient("api");
            var body = new
            {
                NumeroInscricao = request.NumeroInscricao,
                Nome = request.Nome,
                Email = request.Email,
                Senha = request.Senha,
                PalavraVerificacao = request.PalavraVerificacao ?? string.Empty
            };
            var res = await client.PostAsJsonAsync("auth/register/suporte", body, ct);
            if (res.IsSuccessStatusCode) return (true, null);
            var msg = await res.Content.ReadAsStringAsync(ct);
            return (false, NormalizeApiError(msg));
        }
        catch (TaskCanceledException)
        {
            return (false, "Tempo esgotado ao comunicar com a API. Verifique se a API está em execução e o endereço/porta está correto em Configurações.");
        }
        catch (HttpRequestException ex)
        {
            return (false, $"Falha de conexão: {ex.Message}");
        }
    }

    private static string NormalizeApiError(string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload)) return "Falha ao comunicar com a API.";
        try
        {

            using var doc = System.Text.Json.JsonDocument.Parse(payload);
            var root = doc.RootElement;
            if (root.TryGetProperty("title", out var t)) return t.GetString() ?? payload;
            if (root.TryGetProperty("detail", out var d)) return d.GetString() ?? payload;
        }
        catch { }
        return payload;
    }
}
