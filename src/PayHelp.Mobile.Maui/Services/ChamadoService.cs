using System.Net.Http.Json;
using PayHelp.Mobile.Maui.Models;
using Microsoft.Maui.Storage;

namespace PayHelp.Mobile.Maui.Services;

public class ChamadoService
{
    private readonly IHttpClientFactory _factory;

    public ChamadoService(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    public async Task<TicketDto?> CreateAsync(TicketCreateRequest request, CancellationToken ct = default)
    {
        var client = _factory.CreateClient("api");

        var userIdStr = await SecureStorage.Default.GetAsync("user_id");
        if (!Guid.TryParse(userIdStr, out var userId)) return null;
        var body = new { SolicitanteId = userId, request.Titulo, request.Descricao };
        var res = await client.PostAsJsonAsync("chamados", body, ct);
        if (!res.IsSuccessStatusCode) return null;

        var created = await res.Content.ReadFromJsonAsync<CreateResponse>(cancellationToken: ct);
        return created == null ? null : new TicketDto
        {
            Id = created.id,
            Titulo = created.titulo ?? request.Titulo,
            Descricao = request.Descricao,
            Status = created.status ?? string.Empty,
            DataAbertura = created.criadoEmUtc
        };
    }

    public async Task<List<TicketDto>> ListByUserAsync(Guid userId, CancellationToken ct = default)
    {
        var client = _factory.CreateClient("api");
        var list = await client.GetFromJsonAsync<List<ListItemResponse>>($"chamados/usuario/{userId}", ct) ?? new();
        return list.Select(x => new TicketDto
        {
            Id = x.Id,
            Titulo = x.Titulo ?? string.Empty,
            Status = x.Status ?? string.Empty,
            DataAbertura = x.CriadoEmUtc,
            EncerradoEmUtc = x.EncerradoEmUtc,
            Triaging = x.Triaging
        }).ToList();
    }

    public async Task<List<TicketDto>> ListAllAsync(string? status = null, CancellationToken ct = default)
    {
        var client = _factory.CreateClient("api");
        var url = string.IsNullOrWhiteSpace(status) ? "chamados" : $"chamados?status={Uri.EscapeDataString(status)}";
        var list = await client.GetFromJsonAsync<List<ListItemResponse>>(url, ct) ?? new();
        return list.Select(x => new TicketDto
        {
            Id = x.Id,
            Titulo = x.Titulo ?? string.Empty,
            Status = x.Status ?? string.Empty,
            DataAbertura = x.CriadoEmUtc,
            EncerradoEmUtc = x.EncerradoEmUtc,
            Triaging = x.Triaging
        }).ToList();
    }

    public async Task<TicketDto?> GetDetailsAsync(Guid ticketId, CancellationToken ct = default)
    {
        var client = _factory.CreateClient("api");
        var d = await client.GetFromJsonAsync<DetailResponse>($"chamados/{ticketId}", ct);
        if (d == null) return null;
        return new TicketDto
        {
            Id = d.Id,
            Titulo = d.Titulo ?? string.Empty,
            Descricao = d.Descricao ?? string.Empty,
            Status = d.Status ?? string.Empty,
            DataAbertura = d.CriadoEmUtc,
            EncerradoEmUtc = d.EncerradoEmUtc,
            Triaging = d.Triaging,
            ResolucaoFinal = d.ResolucaoFinal,
            Mensagens = (d.Mensagens ?? new())
                .OrderBy(m => m.EnviadoEmUtc)
                .Select(m => new TicketMessageDto
            {
                Id = m.Id,
                Texto = m.Conteudo ?? string.Empty,
                Data = m.EnviadoEmUtc,
                AutorId = m.RemetenteUserId
            }).ToList()
        };
    }

    public async Task<bool> SendMessageAsync(Guid ticketId, TicketMessageRequest msg, bool automatic = false, CancellationToken ct = default)
    {
        var client = _factory.CreateClient("api");
        var userIdStr = await SecureStorage.Default.GetAsync("user_id");
        if (!Guid.TryParse(userIdStr, out var userId)) return false;
        var body = new { RemetenteId = userId, Conteudo = msg.Texto, Automatica = automatic };
        var res = await client.PostAsJsonAsync($"chamados/{ticketId}/mensagens", body, ct);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> ChangeStatusAsync(Guid ticketId, TicketChangeStatusRequest req, CancellationToken ct = default)
    {
        var client = _factory.CreateClient("api");
        var res = await client.PostAsJsonAsync($"chamados/{ticketId}/status", new { NovoStatus = req.Status, SupportUserId = (Guid?)null }, ct);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> CloseAsync(Guid ticketId, CancellationToken ct = default)
    {
        var client = _factory.CreateClient("api");
        var res = await client.PostAsync($"chamados/{ticketId}/encerrar", null, ct);
        return res.IsSuccessStatusCode;
    }

    public async Task<(bool ok, string? error)> CallAtendenteAsync(Guid ticketId, CancellationToken ct = default)
    {
        var client = _factory.CreateClient("api");
        var userIdStr = await SecureStorage.Default.GetAsync("user_id");
        Guid.TryParse(userIdStr, out var userId);

        var res = await client.PostAsJsonAsync($"chamados/{ticketId}/chamar-atendente", new { RemetenteId = userId }, ct);
        if (res.IsSuccessStatusCode) return (true, null);
        var msg = await res.Content.ReadAsStringAsync(ct);
        return (false, string.IsNullOrWhiteSpace(msg) ? "Falha ao chamar atendente." : msg);
    }

    public async Task<bool> AssumirAsync(Guid ticketId, CancellationToken ct = default)
    {
        var client = _factory.CreateClient("api");
        var userIdStr = await SecureStorage.Default.GetAsync("user_id");
        Guid.TryParse(userIdStr, out var supportUserId);
        var res = await client.PostAsJsonAsync($"chamados/{ticketId}/status", new { NovoStatus = "EmAtendimento", SupportUserId = (Guid?)supportUserId }, ct);
        return res.IsSuccessStatusCode;
    }

    public async Task<(bool ok, string? error)> RegistrarResolucaoFinalAsync(Guid ticketId, string solucao, CancellationToken ct = default)
    {
        var client = _factory.CreateClient("api");
        var res = await client.PostAsJsonAsync($"chamados/{ticketId}/resolucao-final", new { Solucao = solucao }, ct);
        if (res.IsSuccessStatusCode) return (true, null);
        var txt = await res.Content.ReadAsStringAsync(ct);
        return (false, string.IsNullOrWhiteSpace(txt) ? "Falha ao registrar a resolução final." : txt);
    }


    private record CreateResponse(Guid id, string? titulo, string? status, DateTime criadoEmUtc);
    private record ListItemResponse
    {
        public Guid Id { get; init; }
        public string? Titulo { get; init; }
        public string? Status { get; init; }
        public DateTime CriadoEmUtc { get; init; }
        public DateTime? EncerradoEmUtc { get; init; }
        public bool Triaging { get; init; }
    }
    private record DetailResponse
    {
        public Guid Id { get; init; }
        public string? Titulo { get; init; }
        public string? Descricao { get; init; }
        public string? Status { get; init; }
        public DateTime CriadoEmUtc { get; init; }
        public DateTime? EncerradoEmUtc { get; init; }
        public List<DetailMessage>? Mensagens { get; init; }
        public bool Triaging { get; init; }
        public string? ResolucaoFinal { get; init; }
    }
    private record DetailMessage
    {
        public Guid Id { get; init; }
        public Guid RemetenteUserId { get; init; }
        public string? Conteudo { get; init; }
        public DateTime EnviadoEmUtc { get; init; }
        public bool Automatica { get; init; }
    }
}
