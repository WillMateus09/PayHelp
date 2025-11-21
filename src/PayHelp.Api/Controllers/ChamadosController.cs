using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PayHelp.Application.Services;
using PayHelp.Domain.Enums;
using PayHelp.Application.Abstractions;
using PayHelp.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace PayHelp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChamadosController : ControllerBase
{
    private readonly ITicketService _tickets;
    private readonly IFaqService _faq;
    private readonly IHubContext<Hubs.ChatHub> _hub;
    private readonly AppDbContext _db;
    
    public ChamadosController(ITicketService tickets, IFaqService faq, IHubContext<Hubs.ChatHub> hub, AppDbContext db)
    { 
        _tickets = tickets; 
        _faq = faq; 
        _hub = hub;
        _db = db;
    }
    

    private static bool IsTriaging(PayHelp.Domain.Entities.Ticket t)
    {
        if (t.Status != TicketStatus.Aberto) return false;
        var cutoff = DateTime.UtcNow.AddMinutes(-10);
        return t.Mensagens != null && t.Mensagens.Any(m => m.Automatica && m.EnviadoEmUtc >= cutoff);
    }

    public record AbrirChamadoRequest(Guid SolicitanteId, string Titulo, string Descricao);
    public record EnviarMensagemRequest(Guid RemetenteId, string Conteudo, bool Automatica);
    public record MudarStatusRequest(string NovoStatus, Guid? SupportUserId);
    public record ResolucaoFinalRequest(string Solucao);
    public record MarcarResolvidoPeloUsuarioRequest(string FeedbackUsuario, int NotaUsuario);
    public record SalvarFeedbackRequest(Guid UserId, int Nota, string? Comentario);

    [HttpPost]
    public async Task<ActionResult<object>> Criar([FromBody] AbrirChamadoRequest req)
    {
        try
        {
            var t = await _tickets.AbrirChamadoAsync(req.SolicitanteId, req.Titulo, req.Descricao);
            return Created($"/api/chamados/{t.Id}", new { id = t.Id, t.Titulo, Status = t.Status.ToString(), t.CriadoEmUtc });
        }
        catch (Exception ex)
        {
            return Problem(ex.Message, statusCode: 400);
        }
    }

    [HttpGet("usuario/{userId}")]
    public async Task<IEnumerable<object>> ListarPorUsuario(Guid userId)
        => (await _tickets.ListarPorUsuarioAsync(userId)).Select(t => new
        {
            t.Id,
            t.Titulo,
            Status = t.Status.ToString(),
            t.CriadoEmUtc,
            t.EncerradoEmUtc,
            Triaging = IsTriaging(t)
        });

    [HttpGet]
    [Authorize(Roles = "Suporte")]
    public async Task<IEnumerable<object>> Listar([FromQuery] string? status)
    {
        TicketStatus? s = status != null && Enum.TryParse<TicketStatus>(status, true, out var parsed) ? parsed : null;
        var list = await _tickets.ListarTodosAsync(s);
        return list.Select(t => new
        {
            t.Id,
            t.Titulo,
            Status = t.Status.ToString(),
            t.CriadoEmUtc,
            t.EncerradoEmUtc,
            Triaging = IsTriaging(t),
            ResolucaoFinal = (string?)null,
            t.ResolvidoPeloUsuario,
            t.FeedbackUsuario,
            t.NotaUsuario,
            t.DataResolvidoUsuario
        });
    }

    [HttpGet("{ticketId}")]
    public async Task<ActionResult<object>> Obter(Guid ticketId)
    {
        var t = await _tickets.ObterPorIdAsync(ticketId);
        if (t == null) return NotFound();
        var faq = await _faq.ObterPorTicketAsync(ticketId);
        return Ok(new
        {
            t.Id,
            t.Titulo,
            t.Descricao,
            Status = t.Status.ToString(),
            t.CriadoEmUtc,
            t.EncerradoEmUtc,
            Mensagens = t.Mensagens.Select(m => new { m.Id, m.RemetenteUserId, m.Conteudo, m.EnviadoEmUtc, m.Automatica }),
            Triaging = IsTriaging(t),
            ResolucaoFinal = faq?.Solucao,
            ResolucaoRegistradaEm = faq?.DataCriacao,
            ResolucaoRegistradaPorUserId = (Guid?)null,
            // Novos campos de resolução pelo usuário
            t.ResolvidoPeloUsuario,
            t.FeedbackUsuario,
            t.NotaUsuario,
            t.DataResolvidoUsuario
        });
    }

    [HttpPost("{ticketId}/mensagens")]
    public async Task<ActionResult<object>> EnviarMensagem(Guid ticketId, [FromBody] EnviarMensagemRequest req)
    {
        try
        {
            var t = await _tickets.EnviarMensagemAsync(ticketId, req.RemetenteId, req.Conteudo, req.Automatica);
            var last = t.Mensagens.Last();
            return Ok(new { last.Id, last.RemetenteUserId, last.Conteudo, last.EnviadoEmUtc, last.Automatica });
        }
        catch (ArgumentException ex)
        {
            return Problem(ex.Message, statusCode: 400);
        }
        catch (KeyNotFoundException ex)
        {
            return Problem(ex.Message, statusCode: 404);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException ex)
        {

            return Problem($"Concorrência ao enviar mensagem: {ex.Message}", statusCode: 409);
        }
        catch (Exception ex)
        {
            return Problem($"Falha ao enviar mensagem: {ex.Message}", statusCode: 500);
        }
    }

    [HttpPost("{ticketId}/status")]
    [Authorize(Roles = "Suporte")]
    public async Task<ActionResult<object>> MudarStatus(Guid ticketId, [FromBody] MudarStatusRequest req)
    {
        if (!Enum.TryParse<TicketStatus>(req.NovoStatus, true, out var status)) return Problem("Status inválido", statusCode: 400);
        var t = await _tickets.MudarStatusAsync(ticketId, status, req.SupportUserId);
        return Ok(new { t.Id, t.Titulo, Status = t.Status.ToString(), t.CriadoEmUtc, t.EncerradoEmUtc });
    }


    [HttpPost("{ticketId}/chamar-atendente")]
    public async Task<ActionResult<object>> ChamarAtendente(Guid ticketId)
    {
        try
        {
            var t = await _tickets.MudarStatusAsync(ticketId, TicketStatus.EmAtendimento, null);
            return Ok(new { t.Id, t.Titulo, Status = t.Status.ToString(), t.CriadoEmUtc, t.EncerradoEmUtc });
        }
        catch (InvalidOperationException ex)
        {
            return Problem(ex.Message, statusCode: 400);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{ticketId}/encerrar")]
    [Authorize(Roles = "Suporte")]
    public async Task<ActionResult<object>> Encerrar(Guid ticketId)
    {
        var t = await _tickets.EncerrarAsync(ticketId);
        return Ok(new { t.Id, t.Titulo, Status = t.Status.ToString(), t.CriadoEmUtc, t.EncerradoEmUtc });
    }

    [HttpPost("{ticketId}/resolucao-final")]
    [Authorize(Roles = "Suporte")]
    public async Task<ActionResult<object>> RegistrarResolucao(Guid ticketId, [FromBody] ResolucaoFinalRequest req)
    {
        var t = await _tickets.ObterPorIdAsync(ticketId);
        if (t == null) return NotFound();
        if (t.Status != TicketStatus.Encerrado)
        {
            return Problem("Ticket precisa estar encerrado antes de registrar a resolução final.", statusCode: 400);
        }

        var created = await _faq.RegistrarAsync(ticketId, t.Titulo, t.Descricao, req.Solucao);

        if (created.TicketId == ticketId && created.DataCriacao < DateTime.UtcNow.AddSeconds(-2))
        {
            return Conflict(new { message = "Já existe resolução final registrada para este ticket.", faqId = created.Id });
        }
        return Ok(new { faqId = created.Id, created.TituloProblema, created.Solucao, created.DataCriacao });
    }

    /// <summary>
    /// Marca o chamado como resolvido pelo usuário (via IA/autoatendimento) - SEM feedback
    /// </summary>
    [HttpPost("{ticketId}/marcar-resolvido-usuario")]
    public async Task<ActionResult<object>> MarcarResolvidoPeloUsuarioSimples(Guid ticketId)
    {
        try
        {
            var t = await _tickets.MarcarComoResolvidoPeloUsuarioAsync(ticketId, "", 0);
            
            // Notificar painel de suporte via SignalR
            await _hub.Clients.Group("support").SendAsync("StatusChanged", new 
            { 
                ticketId = t.Id.ToString(), 
                status = "ResolvidoPeloUsuario",
                resolvidoPeloUsuario = true
            });
            
            return Ok(new 
            { 
                t.Id, 
                t.Titulo, 
                Status = t.Status.ToString(),
                t.ResolvidoPeloUsuario,
                Mensagem = "Chamado marcado como resolvido."
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Chamado não encontrado." });
        }
        catch (Exception ex)
        {
            return Problem($"Erro ao marcar chamado como resolvido: {ex.Message}", statusCode: 500);
        }
    }

    /// <summary>
    /// Salva o feedback do usuário para um chamado
    /// </summary>
    [HttpPost("{ticketId}/feedback")]
    public async Task<ActionResult<object>> SalvarFeedback(Guid ticketId, [FromBody] SalvarFeedbackRequest req)
    {
        try
        {
            var t = await _db.Tickets.FindAsync(ticketId);
            if (t == null) return NotFound(new { message = "Chamado não encontrado." });

            // Atualizar o feedback no ticket
            t.FeedbackUsuario = req.Comentario ?? "";
            t.NotaUsuario = req.Nota;
            
            if (!t.DataResolvidoUsuario.HasValue)
            {
                t.DataResolvidoUsuario = DateTime.UtcNow;
            }

            _db.Tickets.Update(t);
            await _db.SaveChangesAsync();

            return Ok(new 
            { 
                TicketId = t.Id,
                Nota = t.NotaUsuario,
                Comentario = t.FeedbackUsuario,
                DataCriacao = t.DataResolvidoUsuario,
                Mensagem = "Feedback salvo com sucesso!"
            });
        }
        catch (Exception ex)
        {
            return Problem($"Erro ao salvar feedback: {ex.Message}", statusCode: 500);
        }
    }

    /// <summary>
    /// Obtém o feedback do usuário para um chamado
    /// </summary>
    [HttpGet("{ticketId}/feedback")]
    public async Task<ActionResult<object>> ObterFeedback(Guid ticketId)
    {
        try
        {
            var t = await _tickets.ObterPorIdAsync(ticketId);
            if (t == null) return NotFound(new { message = "Chamado não encontrado." });

            if (!t.NotaUsuario.HasValue)
            {
                return NotFound(new { message = "Nenhum feedback encontrado para este chamado." });
            }

            var resultado = new 
            { 
                Id = t.Id,
                TicketId = t.Id,
                UserId = t.UserId,
                Nota = t.NotaUsuario.Value,
                Comentario = t.FeedbackUsuario ?? "",
                CriadoEmUtc = t.DataResolvidoUsuario ?? t.CriadoEmUtc
            };

            Console.WriteLine($"[FEEDBACK DEBUG] Retornando feedback: Nota={resultado.Nota}, Comentario='{resultado.Comentario}'");
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return Problem($"Erro ao obter feedback: {ex.Message}", statusCode: 500);
        }
    }

    /// <summary>
    /// Obtém o chamado completo com feedback
    /// </summary>
    [HttpGet("{ticketId}/completo")]
    public async Task<ActionResult<object>> ObterChamadoCompleto(Guid ticketId)
    {
        try
        {
            var t = await _tickets.ObterPorIdAsync(ticketId);
            if (t == null) return NotFound(new { message = "Chamado não encontrado." });

            var faq = await _faq.ObterPorTicketAsync(ticketId);

            return Ok(new 
            { 
                t.Id,
                t.Titulo,
                t.Descricao,
                Status = t.Status.ToString(),
                t.CriadoEmUtc,
                t.EncerradoEmUtc,
                Mensagens = t.Mensagens.Select(m => new 
                { 
                    m.Id, 
                    m.RemetenteUserId, 
                    m.Conteudo, 
                    m.EnviadoEmUtc, 
                    m.Automatica 
                }),
                Feedback = t.NotaUsuario.HasValue ? new 
                {
                    nota = t.NotaUsuario.Value,
                    comentario = t.FeedbackUsuario ?? "",
                    dataCriacao = t.DataResolvidoUsuario ?? t.CriadoEmUtc
                } : null,
                ResolucaoFinal = faq?.Solucao,
                t.ResolvidoPeloUsuario
            });
        }
        catch (Exception ex)
        {
            return Problem($"Erro ao obter chamado: {ex.Message}", statusCode: 500);
        }
    }

    /// <summary>
    /// Lista todos os feedbacks dos usuários
    /// </summary>
    [HttpGet("feedbacks")]
    public async Task<ActionResult<List<object>>> ListarFeedbacks()
    {
        try
        {
            var ticketsComFeedback = await _db.Tickets
                .Where(t => t.NotaUsuario.HasValue && t.NotaUsuario.Value > 0)
                .OrderByDescending(t => t.DataResolvidoUsuario ?? t.CriadoEmUtc)
                .ToListAsync();

            // Buscar informações dos usuários
            var userIds = ticketsComFeedback.Select(t => t.UserId).Distinct().ToList();
            var users = await _db.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();
            var usersDict = users.ToDictionary(u => u.Id);

            var resultado = ticketsComFeedback.Select(t =>
            {
                var user = usersDict.TryGetValue(t.UserId, out var u) ? u : null;
                return new
                {
                    TicketId = t.Id,
                    TicketTitulo = t.Titulo ?? "Sem título",
                    UsuarioNome = user?.Nome ?? "Desconhecido",
                    UsuarioEmail = user?.Email ?? "",
                    Nota = t.NotaUsuario!.Value,
                    Comentario = t.FeedbackUsuario ?? "(sem comentário)",
                    DataCriacao = t.DataResolvidoUsuario ?? t.CriadoEmUtc
                };
            }).ToList();

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return Problem($"Erro ao listar feedbacks: {ex.Message}", statusCode: 500);
        }
    }

    /// <summary>
    /// Marca o chamado como resolvido pelo usuário (via IA/autoatendimento)
    /// </summary>
    [HttpPatch("{ticketId}/marcar-resolvido-usuario")]
    public async Task<ActionResult<object>> MarcarResolvidoPeloUsuario(Guid ticketId, [FromBody] MarcarResolvidoPeloUsuarioRequest req)
    {
        try
        {
            var t = await _tickets.MarcarComoResolvidoPeloUsuarioAsync(ticketId, req.FeedbackUsuario, req.NotaUsuario);
            
            // Notificar painel de suporte via SignalR
            await _hub.Clients.Group("support").SendAsync("StatusChanged", new 
            { 
                ticketId = t.Id.ToString(), 
                status = "ResolvidoPeloUsuario",
                resolvidoPeloUsuario = true,
                feedbackUsuario = t.FeedbackUsuario,
                notaUsuario = t.NotaUsuario,
                dataResolvidoUsuario = t.DataResolvidoUsuario
            });
            
            return Ok(new 
            { 
                t.Id, 
                t.Titulo, 
                Status = t.Status.ToString(),
                t.ResolvidoPeloUsuario,
                t.FeedbackUsuario,
                t.NotaUsuario,
                t.DataResolvidoUsuario,
                Mensagem = "Chamado marcado como resolvido. Obrigado pelo feedback!"
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Chamado não encontrado." });
        }
        catch (InvalidOperationException ex)
        {
            return Problem(ex.Message, statusCode: 400);
        }
        catch (ArgumentException ex)
        {
            return Problem(ex.Message, statusCode: 400);
        }
        catch (Exception ex)
        {
            return Problem($"Erro ao marcar chamado como resolvido: {ex.Message}", statusCode: 500);
        }
    }

    /// <summary>
    /// Obtém estatísticas de feedback dos usuários para o dashboard do suporte
    /// </summary>
    [HttpGet("feedbacks/estatisticas")]
    [Authorize(Roles = "Suporte")]
    public async Task<ActionResult<List<FeedbackEstatisticaResponseDto>>> ObterEstatisticasFeedbacks()
    {
        Console.WriteLine("[ChamadosController] ===== INICIO ObterEstatisticasFeedbacks =====");
        
        var tickets = await _tickets.ListarTodosAsync(TicketStatus.ResolvidoPeloUsuario);
        Console.WriteLine($"[ChamadosController] Tickets com ResolvidoPeloUsuario: {tickets.Count()}");
        
        var feedbacks = tickets
            .Where(t => t.ResolvidoPeloUsuario && t.NotaUsuario.HasValue)
            .GroupBy(t => t.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                MediaNotas = g.Average(t => t.NotaUsuario ?? 0),
                UltimaNota = g.OrderByDescending(t => t.DataResolvidoUsuario).First().NotaUsuario ?? 0,
                UltimoFeedback = g.OrderByDescending(t => t.DataResolvidoUsuario).First().FeedbackUsuario ?? "",
                TotalChamados = g.Count()
            })
            .OrderByDescending(f => f.MediaNotas)
            .Take(10)
            .ToList();

        Console.WriteLine($"[ChamadosController] Total de feedbacks agrupados: {feedbacks.Count}");
        
        // Buscar informações dos usuários
        var userIds = feedbacks.Select(f => f.UserId).ToList();
        Console.WriteLine($"[ChamadosController] UserIds para buscar: {string.Join(", ", userIds)}");
        
        // Testar quantos usuários existem no total
        var totalUsers = await _db.Users.CountAsync();
        Console.WriteLine($"[ChamadosController] Total de usuários no banco: {totalUsers}");
        
        var users = await _db.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();
        Console.WriteLine($"[ChamadosController] Usuários encontrados no banco: {users.Count}");
        
        foreach (var user in users)
        {
            Console.WriteLine($"[ChamadosController] User: {user.Id} - {user.Nome} ({user.Email})");
        }
        
        // Se não encontrou usuários, buscar TODOS os usuários para debug
        if (users.Count == 0)
        {
            var allUsers = await _db.Users.Take(5).ToListAsync();
            Console.WriteLine($"[ChamadosController] Primeiros 5 usuários do banco:");
            foreach (var u in allUsers)
            {
                Console.WriteLine($"[ChamadosController]   - {u.Id} | {u.Nome} | {u.Email}");
            }
        }
        
        var resultado = feedbacks.Select(f => 
        {
            var user = users.FirstOrDefault(u => u.Id == f.UserId);
            var userName = user == null ? "" : (string.IsNullOrWhiteSpace(user.Nome) ? user.Email : user.Nome);
            var userEmail = user?.Email ?? "";
            Console.WriteLine($"[ChamadosController] Feedback UserId={f.UserId}: UserName='{userName}', UserEmail='{userEmail}'");
            return new FeedbackEstatisticaResponseDto
            {
                UserId = f.UserId,
                UserName = userName,
                UserEmail = userEmail,
                MediaNotas = f.MediaNotas,
                UltimaNota = f.UltimaNota,
                UltimoFeedback = f.UltimoFeedback,
                TotalChamados = f.TotalChamados
            };
        }).ToList();

        Console.WriteLine("[ChamadosController] ===== FIM ObterEstatisticasFeedbacks =====");
        return Ok(resultado);
    }
}

public class FeedbackEstatisticaResponseDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public double MediaNotas { get; set; }
    public int UltimaNota { get; set; }
    public string UltimoFeedback { get; set; } = string.Empty;
    public int TotalChamados { get; set; }
}
