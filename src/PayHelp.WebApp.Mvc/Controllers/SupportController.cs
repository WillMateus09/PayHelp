using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PayHelp.Domain.Enums;
using PayHelp.WebApp.Mvc.Hubs;
using PayHelp.WebApp.Mvc.Services;
using PayHelp.WebApp.Mvc.ViewModels;

namespace PayHelp.WebApp.Mvc.Controllers;

[Authorize(Roles = "Suporte")]
public class SupportController : Controller
{
    private readonly ApiService _api;
    private readonly IHubContext<ChatHub> _hub;
    private readonly ITriageTracker _triageTracker;

    public SupportController(ApiService api, IHubContext<ChatHub> hub, ITriageTracker triageTracker)
    {
        _api = api;
        _hub = hub;
        _triageTracker = triageTracker;
    }


    private record TicketListDto(
        Guid Id,
        string Titulo,
        string Status,
        DateTime CriadoEmUtc,
        DateTime? EncerradoEmUtc,
        string? ResolucaoFinal,
        bool? Triaging
    );

    private record TicketDetailsDto(
        Guid Id,
        string Titulo,
        string Status,
        DateTime CriadoEmUtc,
        DateTime? EncerradoEmUtc,
        IEnumerable<MessageDto> Mensagens,
        string? ResolucaoFinal,
        DateTime? ResolucaoRegistradaEm,
        Guid? ResolucaoRegistradaPorUserId
    );

    private record MessageDto(Guid Id, Guid RemetenteUserId, string Conteudo, DateTime EnviadoEmUtc, bool Automatica);

    public async Task<IActionResult> Dashboard(string? status)
    {
        var endpoint = "chamados" + (string.IsNullOrWhiteSpace(status) ? string.Empty : $"?status={status}");
        var tickets = await _api.GetAsync<List<TicketListDto>>(endpoint) ?? new();


        string? Trunc(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            const int max = 120;
            return s.Length > max ? s.Substring(0, max) + "..." : s;
        }

    var vm = new SupportDashboardViewModel
        {
            Abertos = tickets
                .Where(t => t.Status == TicketStatus.Aberto.ToString())
                .Select(t => new TicketListItemViewModel
                {
                    Id = t.Id,
                    Titulo = t.Titulo,
                    Status = TicketStatus.Aberto,
                    CriadoEmUtc = t.CriadoEmUtc,
                    EncerradoEmUtc = t.EncerradoEmUtc,
                    PossuiResolucao = !string.IsNullOrWhiteSpace(t.ResolucaoFinal),
                    ResumoResolucao = Trunc(t.ResolucaoFinal)
                }).ToList(),

            EmAtendimento = tickets
                .Where(t => t.Status == TicketStatus.EmAtendimento.ToString())
                .Select(t => new TicketListItemViewModel
                {
                    Id = t.Id,
                    Titulo = t.Titulo,
                    Status = TicketStatus.EmAtendimento,
                    CriadoEmUtc = t.CriadoEmUtc,
                    EncerradoEmUtc = t.EncerradoEmUtc,
                    PossuiResolucao = !string.IsNullOrWhiteSpace(t.ResolucaoFinal),
                    ResumoResolucao = Trunc(t.ResolucaoFinal)
                }).ToList(),

            Encerrados = tickets
                .Where(t => t.Status == TicketStatus.Encerrado.ToString())
                .Select(t => new TicketListItemViewModel
                {
                    Id = t.Id,
                    Titulo = t.Titulo,
                    Status = TicketStatus.Encerrado,
                    CriadoEmUtc = t.CriadoEmUtc,
                    EncerradoEmUtc = t.EncerradoEmUtc,
                    PossuiResolucao = !string.IsNullOrWhiteSpace(t.ResolucaoFinal),
                    ResumoResolucao = Trunc(t.ResolucaoFinal)
                }).ToList(),

            FiltroSelecionado = string.IsNullOrWhiteSpace(status)
                ? null
                : Enum.TryParse<TicketStatus>(status, true, out var parsed) ? parsed : null,

            TriagingIds = BuildTriagingSet(tickets)
        };
        return View(vm);
    }

    private HashSet<Guid> BuildTriagingSet(List<TicketListDto> tickets)
    {

        var set = _triageTracker.GetAllTriaging().ToHashSet();
        foreach (var t in tickets)
        {
            if (string.Equals(t.Status, TicketStatus.Aberto.ToString(), StringComparison.OrdinalIgnoreCase)
                && (t.Triaging ?? false))
            {
                set.Add(t.Id);
            }
        }
        return set;
    }

    public async Task<IActionResult> Chat(Guid id)
    {
        var dto = await _api.GetAsync<TicketDetailsDto>($"chamados/{id}");
        if (dto == null) return NotFound();
        var currentUserId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var currentUserName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Suporte";
        var statusEnum = Enum.TryParse<TicketStatus>(dto.Status, out var st) ? st : TicketStatus.Aberto;

        var chatVm = new ChatViewModel
        {
            TicketId = dto.Id,
            Titulo = dto.Titulo,
            Status = statusEnum,
            Mensagens = dto.Mensagens
                .OrderBy(m => m.EnviadoEmUtc)
                .Select(m => new ChatMessageVM
                {
                    RemetenteUserId = m.RemetenteUserId,
                    RemetenteNome = m.Automatica ? "BOT" : (m.RemetenteUserId == currentUserId ? currentUserName : "Cliente"),
                    Conteudo = m.Conteudo,
                    EnviadoEm = m.EnviadoEmUtc.ToLocalTime(),
                    Automatica = m.Automatica
                }).ToList()
        };

        return View("~/Views/Tickets/Chat.cshtml", chatVm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnviarMensagem(Guid ticketId, string mensagem)
    {
        mensagem = mensagem?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(mensagem))
        {
            TempData["Erro"] = "Digite uma mensagem antes de enviar.";
            return RedirectToAction("Chat", new { id = ticketId });
        }
        var currentUserId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var body = new { RemetenteId = currentUserId, Conteudo = mensagem, Automatica = false };
        try
        {
            await _api.PostAsync<object, object>($"chamados/{ticketId}/mensagens", body);
            await _hub.Clients.Group(ticketId.ToString()).SendAsync("ReceiveMessage", new
            {
                ticketId = ticketId.ToString(),
                user = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Suporte",
                message = mensagem,
                sentAt = DateTimeOffset.UtcNow
            });
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction("Chat", new { id = ticketId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Encerrar(Guid ticketId)
    {
        try
        {
            await _api.PostAsync<object, object>($"chamados/{ticketId}/encerrar", new { });
            return RedirectToAction("ResolucaoFinal", new { ticketId });
        }
        catch (Exception ex)
        {
            TempData["Erro"] = ex.Message;
            return RedirectToAction("Chat", new { id = ticketId });
        }
    }

    public async Task<IActionResult> ResolucaoFinal(Guid ticketId)
    {
        var dto = await _api.GetAsync<TicketDetailsDto>($"chamados/{ticketId}");
        if (dto == null) return NotFound();

        if (!string.Equals(dto.Status, TicketStatus.Encerrado.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("Chat", new { id = ticketId });
        }

        ViewBag.TicketTitulo = dto.Titulo;
        ViewBag.Resolucao = dto.ResolucaoFinal;
        ViewBag.ResolucaoRegistradaEm = dto.ResolucaoRegistradaEm;


        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarResolucao(Guid ticketId, string solucao)
    {

        try
        {
            var atual = await _api.GetAsync<TicketDetailsDto>($"chamados/{ticketId}");
            if (atual != null && !string.IsNullOrWhiteSpace(atual.ResolucaoFinal))
            {
                TempData["Erro"] = "Este ticket já possui resolução final registrada. Edição não permitida.";
                return RedirectToAction("ResolucaoFinal", new { ticketId });
            }
        }
        catch {  }

        string textoDireto = solucao?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(textoDireto))
        {
            TempData["Erro"] = "Informe a descrição da solução.";
            return RedirectToAction("ResolucaoFinal", new { ticketId });
        }
        var mensagemBot = $"Resolução final: {textoDireto}";
        bool sucessoApi = false;
        try
        {
            await _api.PostAsync<object, object>($"chamados/{ticketId}/resolucao-final", new { Solucao = textoDireto });
            sucessoApi = true;
        }
        catch (Exception ex)
        {

            TempData["AvisoResolucaoLocal"] = "Endpoint de resolução não encontrado (404). Registrado apenas no chat.";
            TempData["Erro"] = ex.Message.Contains("404") ? null : ex.Message;
        }

        try
        {

            await _api.PostAsync<object, object>($"chamados/{ticketId}/mensagens", new { RemetenteId = Guid.Empty, Conteudo = mensagemBot, Automatica = true });
            await _hub.Clients.Group(ticketId.ToString()).SendAsync("ReceiveMessage", new
            {
                ticketId = ticketId.ToString(),
                user = "BOT",
                message = mensagemBot,
                sentAt = DateTimeOffset.UtcNow
            });
        }
        catch {  }

        if (sucessoApi)
            TempData["Sucesso"] = "Resolução final registrada.";

        return RedirectToAction("ResolucaoFinal", new { ticketId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assumir(Guid id)
    {
        try
        {
            await _api.PostAsync<object, object>($"chamados/{id}/assumir", new { });
        }
        catch
        {
            try { await _api.PostAsync<object, object>($"chamados/{id}/status", new { Status = "EmAtendimento" }); } catch { }
        }
        return RedirectToAction("Chat", new { id });
    }
}
