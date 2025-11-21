using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PayHelp.Domain.Enums;
using PayHelp.WebApp.Mvc.Hubs;
using PayHelp.WebApp.Mvc.Services;
using PayHelp.WebApp.Mvc.ViewModels;
using PayHelp.WebApp.Mvc.Models;

namespace PayHelp.WebApp.Mvc.Controllers;

[Authorize]
public class TicketsController : Controller
{
    private readonly IHubContext<ChatHub> _hub;
    private readonly ITriageTracker _triageTracker;
    private readonly ApiService _api;



    private sealed record FaqItemDto(int Id, string? TituloProblema, string? Solucao, DateTime DataCriacao, Guid? TicketId);
    private sealed record TriagemApiResponse(string? Sugestao, string? Origem, string? Faq);

    public TicketsController(IHubContext<ChatHub> hub, ITriageTracker triageTracker, ApiService api)
    {
        _hub = hub;
        _triageTracker = triageTracker;
        _api = api;
    }


    private record TicketListDto(Guid Id, string Titulo, string Status, DateTime CriadoEmUtc, DateTime? EncerradoEmUtc);
    private record MessageDto(Guid Id, Guid RemetenteUserId, string Conteudo, DateTime EnviadoEmUtc, bool Automatica);
    private record TicketDetailsDto(Guid Id, string Titulo, string Descricao, string Status, DateTime CriadoEmUtc, DateTime? EncerradoEmUtc, IEnumerable<MessageDto> Mensagens, string? FeedbackUsuario, int? NotaUsuario);

    [HttpGet]
    public async Task<IActionResult> Minhas()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var lista = await _api.GetAsync<List<TicketListDto>>($"chamados/usuario/{userId}") ?? new();
        return View(lista);
    }

    [HttpGet]
    public IActionResult Abrir() => View(new AbrirChamadoViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Abrir(AbrirChamadoViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var created = await _api.PostAsync<object, CreateTicketResponse>("chamados", new { SolicitanteId = userId, Titulo = vm.Titulo!, Descricao = vm.Descricao! });
        if (created == null || created.Id == Guid.Empty)
        {
            ModelState.AddModelError(string.Empty, "Falha ao criar chamado na API.");
            return View(vm);
        }


        try
        {
            var resultados = await _api.PostAsync<object, List<FaqItemDto>>("faq/buscar", new { Texto = vm.Descricao! })
                              ?? new List<FaqItemDto>();
            var top = resultados.FirstOrDefault();
            string? respostaFaq = top?.Solucao;


            string textoAutomatico = !string.IsNullOrWhiteSpace(respostaFaq)
                ? $"FAQ: {respostaFaq!.Trim()}"
                : "Não encontramos uma solução para seu problema no banco de resoluções antigas (FAQ). Em seguida, utilize a Triagem IA para buscar soluções documentadas pelo suporte.";

            await _api.PostAsync<object, object>($"chamados/{created.Id}/mensagens", new { RemetenteId = userId, Conteudo = textoAutomatico, Automatica = true });
        }
        catch
        {


        }
        return RedirectToAction("Chat", new { id = created.Id });
    }

    private sealed class CreateTicketResponse
    {
        public Guid Id { get; set; }
        public string? Titulo { get; set; }
        public string? Status { get; set; }
        public DateTime CriadoEmUtc { get; set; }
    }

    [HttpGet]
    public async Task<IActionResult> Detalhes(Guid id)
    {
        var dto = await _api.GetAsync<TicketDetailsDto>($"chamados/{id}");
        if (dto == null) return NotFound();
        var statusEnum = Enum.TryParse<TicketStatus>(dto.Status, true, out var st) ? st : TicketStatus.Aberto;
        var vm = new TicketDetailsViewModel
        {
            Id = id,
            Titulo = dto.Titulo,
            Descricao = dto.Descricao,
            Status = statusEnum,
            CriadoEmUtc = dto.CriadoEmUtc,
            EncerradoEmUtc = dto.EncerradoEmUtc
        };
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Chat(Guid id)
    {
        var dto = await _api.GetAsync<TicketDetailsDto>($"chamados/{id}");
        if (dto == null) return NotFound();
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var currentUserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Você";
        var isSupport = User.IsInRole("Suporte");
        var statusEnum = Enum.TryParse<TicketStatus>(dto.Status, true, out var st) ? st : TicketStatus.Aberto;
        var vm = new ChatViewModel
        {
            TicketId = dto.Id,
            Titulo = dto.Titulo,
            Status = statusEnum,
            FeedbackUsuario = dto.FeedbackUsuario,
            NotaUsuario = dto.NotaUsuario,
            Mensagens = (dto.Mensagens ?? Array.Empty<MessageDto>())
                .OrderBy(m => m.EnviadoEmUtc)
                .Select(m => new ChatMessageVM
                {
                    RemetenteUserId = m.RemetenteUserId,
                    RemetenteNome = m.Automatica ? "BOT" : (m.RemetenteUserId == currentUserId ? currentUserName : (isSupport ? "Cliente" : "Suporte")),
                    Conteudo = m.Conteudo,
                    EnviadoEm = m.EnviadoEmUtc.ToLocalTime(),
                    Automatica = m.Automatica
                }).ToList()
        };
        return View(vm);
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
        var dto = await _api.GetAsync<TicketDetailsDto>($"chamados/{ticketId}");
        if (dto == null) return NotFound();
        if (string.Equals(dto.Status, TicketStatus.Encerrado.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            TempData["Erro"] = "N�o � poss�vel enviar mensagens em chamados encerrados.";
            return RedirectToAction("Chat", new { id = ticketId });
        }

        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        await _api.PostAsync<object, object>($"chamados/{ticketId}/mensagens", new { RemetenteId = userId, Conteudo = mensagem, Automatica = false });

        var currentUserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Voc�";
        await _hub.Clients.Group(ticketId.ToString()).SendAsync("ReceiveMessage", new
        {
            ticketId = ticketId.ToString(),
            user = currentUserName,
            message = mensagem,
            sentAt = DateTimeOffset.UtcNow
        });

        return RedirectToAction("Chat", new { id = ticketId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TriagemIA(Guid ticketId)
    {
        var dto = await _api.GetAsync<TicketDetailsDto>($"chamados/{ticketId}");
        if (dto == null) return NotFound();

        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        HttpContext.Session.SetString($"triage:{ticketId}", "1");
        var alreadyTriaging = _triageTracker.IsTriaging(ticketId);
        _triageTracker.MarkStarted(ticketId);
        if (!alreadyTriaging)
        {
            await _hub.Clients.Group("support").SendAsync("TriageStarted", new
            {
                ticketId = ticketId.ToString(),
                title = dto.Titulo
            });
        }


        string descricao = (dto.Mensagens ?? Array.Empty<MessageDto>())
            .Where(m => m != null && !m.Automatica && m.RemetenteUserId == userId)
            .OrderByDescending(m => m.EnviadoEmUtc)
            .Select(m => m.Conteudo)
            .FirstOrDefault()
            ?? (dto.Descricao ?? string.Empty);

        string? resposta = null;
        try
        {
            var tri = await _api.PostAsync<object, TriagemApiResponse>("triagem", new { Texto = descricao });
            resposta = tri?.Sugestao ?? tri?.Faq ?? string.Empty;
        }
        catch { }

        var mensagemBot = string.IsNullOrWhiteSpace(resposta)
            ? "Não há problemas parecidos com soluções prontas no momento. Você pode chamar um atendente para ajudá-lo."
            : $"Triagem IA: {resposta}";

        await _api.PostAsync<object, object>($"chamados/{ticketId}/mensagens", new { RemetenteId = userId, Conteudo = mensagemBot, Automatica = true });

        await _hub.Clients.Group(ticketId.ToString()).SendAsync("ReceiveMessage", new
        {
            ticketId = ticketId.ToString(),
            user = "BOT",
            message = mensagemBot,
            sentAt = DateTimeOffset.UtcNow
        });

        return RedirectToAction("Chat", new { id = ticketId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChamarAtendente(Guid ticketId)
    {
        if (!HttpContext.Session.TryGetValue($"triage:{ticketId}", out _))
        {
            TempData["TriagemObrigatoria"] = "Antes de chamar um atendente, execute a Triagem IA. Ela pode resolver seu problema mais r�pido.";
            return RedirectToAction("Chat", new { id = ticketId });
        }


        var dto = await _api.GetAsync<TicketDetailsDto>($"chamados/{ticketId}");
        if (dto == null) return NotFound();
        if (!string.Equals(dto.Status, TicketStatus.Aberto.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            TempData["Erro"] = "S� � poss�vel chamar atendente quando o chamado est� Aberto.";
            return RedirectToAction("Chat", new { id = ticketId });
        }

        try
        {
            await _api.PostAsync<object, object>($"chamados/{ticketId}/chamar-atendente", new { });
            _triageTracker.Clear(ticketId);
            await _hub.Clients.Group("support").SendAsync("TriageCleared", new { ticketId = ticketId.ToString() });
            await _hub.Clients.Group("support").SendAsync("StatusChanged", new { ticketId = ticketId.ToString(), status = "EmAtendimento", viaApi = true });
        }
        catch (HttpRequestException ex)
        {

            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction("Chat", new { id = ticketId });
    }

    [HttpPost]
    [Authorize(Roles = "Suporte")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Encerrar(Guid ticketId)
    {
        try
        {
            await _api.PostAsync<object, object>($"chamados/{ticketId}/encerrar", new { });
            _triageTracker.Clear(ticketId);
            await _hub.Clients.Group("support").SendAsync("TriageCleared", new { ticketId = ticketId.ToString() });
            await _hub.Clients.Group("support").SendAsync("StatusChanged", new { ticketId = ticketId.ToString(), status = "Encerrado", viaApi = true });
        }
        catch (UnauthorizedAccessException ex)
        {
            TempData["Erro"] = ex.Message;
        }
        catch (HttpRequestException ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction("Chat", new { id = ticketId });
    }

    [HttpGet]
    [Authorize(Roles = "Simples")]
    public async Task<IActionResult> MarcarResolvido(Guid id)
    {
        var dto = await _api.GetAsync<TicketDetailsDto>($"chamados/{id}");
        if (dto == null) return NotFound();

        if (string.Equals(dto.Status, TicketStatus.Encerrado.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            TempData["Erro"] = "Este chamado já está encerrado.";
            return RedirectToAction("Chat", new { id });
        }

        var vm = new MarcarResolvidoViewModel
        {
            TicketId = id,
            TituloTicket = dto.Titulo
        };

        return View(vm);
    }

    [HttpPost]
    [Authorize(Roles = "Simples")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarResolvido(MarcarResolvidoViewModel vm)
    {
        Console.WriteLine("=== POST MarcarResolvido RECEBIDO ===");
        Console.WriteLine($"TicketId: {vm.TicketId}");
        Console.WriteLine($"NotaUsuario: {vm.NotaUsuario}");
        Console.WriteLine($"FeedbackUsuario: {(vm.FeedbackUsuario != null ? vm.FeedbackUsuario.Substring(0, Math.Min(50, vm.FeedbackUsuario.Length)) : "null")}");
        Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
        
        if (!ModelState.IsValid)
        {
            Console.WriteLine("❌ ModelState INVÁLIDO:");
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"  - {error.ErrorMessage}");
            }
            return View(vm);
        }

        try
        {
            Console.WriteLine("✓ Chamando API...");
            Console.WriteLine($"Endpoint completo: chamados/{vm.TicketId}/marcar-resolvido-usuario");
            
            var resultado = await _api.PatchAsync<object>($"chamados/{vm.TicketId}/marcar-resolvido-usuario", new
            {
                FeedbackUsuario = vm.FeedbackUsuario,
                NotaUsuario = vm.NotaUsuario
            });

            Console.WriteLine("✓ API respondeu com sucesso!");
            Console.WriteLine($"Resultado: {System.Text.Json.JsonSerializer.Serialize(resultado)}");
            
            TempData["Sucesso"] = "✓ Chamado marcado como resolvido! Obrigado pelo seu feedback.";
            Console.WriteLine("✓ Redirecionando para Minhas...");
            return RedirectToAction("Minhas");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"❌ ERRO HTTP na API: {ex.Message}");
            Console.WriteLine($"StatusCode: {ex.StatusCode}");
            Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
            
            var mensagemErro = ex.Message.Contains("404") 
                ? "Erro ao marcar como resolvido: Endpoint não encontrado. A API está rodando?" 
                : $"Erro ao marcar como resolvido: {ex.Message}";
                
            ModelState.AddModelError("", mensagemErro);
            return View(vm);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERRO GERAL: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            ModelState.AddModelError("", $"Erro inesperado: {ex.Message}");
            return View(vm);
        }
    }
}
