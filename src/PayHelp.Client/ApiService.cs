using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PayHelp.Client.Dtos;

namespace PayHelp.Client;

public sealed class ApiService
{
    private readonly HttpClient _http;
    private readonly ITokenStore _tokenStore;
    private readonly string _baseUrl;

    public ApiService(HttpClient http, IOptions<ApiOptions> options, ITokenStore tokenStore)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _tokenStore = tokenStore ?? throw new ArgumentNullException(nameof(tokenStore));
        var rawBase = options?.Value?.BaseUrl;
        _baseUrl = ApiBaseUrlHelper.NormalizeBaseUrl(rawBase);
        if (_http.BaseAddress is null)
        {
            _http.BaseAddress = new Uri(_baseUrl);
        }
    }

    public async Task<string> LoginAsync(string email, string senha, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("auth/login", new AuthRequest { Email = email, Senha = senha }, ct).ConfigureAwait(false);
        if (!resp.IsSuccessStatusCode)
        {
            if ((int)resp.StatusCode is 401 or 403)
                throw new ApiUnauthorizedException(statusCode: (int)resp.StatusCode);
            resp.EnsureSuccessStatusCode();
        }

        string token = string.Empty;
        var content = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(content) && content.TrimStart().StartsWith("{"))
        {
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;
            if (root.TryGetProperty("token", out var t)) token = t.GetString() ?? string.Empty;
            else if (root.TryGetProperty("accessToken", out t)) token = t.GetString() ?? string.Empty;
            else if (root.TryGetProperty("access_token", out t)) token = t.GetString() ?? string.Empty;
        }
        else
        {
            token = content.Trim(); // plain text token
        }

        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("A API não retornou um token de autenticação esperado.");

        return token;
    }

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        // Opcional: se a API possuir endpoint de logout, chame aqui.
        await _tokenStore.ClearAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<TicketDto>> GetTicketsAsync(CancellationToken ct = default)
    {
        var result = await _http.GetFromJsonAsync<IEnumerable<TicketDto>>("tickets", cancellationToken: ct).ConfigureAwait(false);
        return result ?? Enumerable.Empty<TicketDto>();
    }

    public async Task<TicketDto> ResolverPeloUsuarioAsync(Guid ticketId, Guid usuarioId, string? feedback, int? nota, CancellationToken ct = default)
    {
        // Ajustar rota para a rota real da API.
        var route = $"tickets/{ticketId}/resolver-usuario"; // POST esperado
        var payload = new ResolveTicketRequest { UsuarioId = usuarioId, Feedback = feedback, Nota = nota };
        var resp = await _http.PostAsJsonAsync(route, payload, ct).ConfigureAwait(false);
        if (!resp.IsSuccessStatusCode)
        {
            if ((int)resp.StatusCode is 401 or 403)
                throw new ApiUnauthorizedException(statusCode: (int)resp.StatusCode);
            resp.EnsureSuccessStatusCode();
        }
        var dto = await resp.Content.ReadFromJsonAsync<TicketDto>(cancellationToken: ct).ConfigureAwait(false);
        return dto ?? new TicketDto { Id = ticketId, Status = "RESOLVIDO" };
    }

    public async Task<IEnumerable<TicketFeedbackDto>> GetFeedbacksDoTicketAsync(Guid ticketId, CancellationToken ct = default)
    {
        var route = $"tickets/{ticketId}/feedbacks";
        var items = await _http.GetFromJsonAsync<IEnumerable<TicketFeedbackDto>>(route, ct).ConfigureAwait(false);
        return items ?? Enumerable.Empty<TicketFeedbackDto>();
    }

    public async Task<string?> GetServerVersionRawAsync(CancellationToken ct = default)
    {
        // Chama GET /__version se existir
        var resp = await _http.GetAsync("__version", ct).ConfigureAwait(false);
        if (!resp.IsSuccessStatusCode)
            return null;
        return await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
    }
}
