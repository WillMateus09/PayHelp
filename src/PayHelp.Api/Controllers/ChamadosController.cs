using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayHelp.Application.Services;
using PayHelp.Domain.Enums;
using PayHelp.Application.Abstractions;

namespace PayHelp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChamadosController : ControllerBase
{
    private readonly ITicketService _tickets;
    private readonly IFaqService _faq;
    public ChamadosController(ITicketService tickets, IFaqService faq){ _tickets = tickets; _faq = faq; }
    

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
            Triaging = IsTriaging(t)
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
            ResolucaoRegistradaPorUserId = (Guid?)null
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
}
