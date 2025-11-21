using Microsoft.AspNetCore.Mvc;
using PayHelp.Application.Abstractions;
using PayHelp.Domain.Enums;

namespace PayHelp.WebApp.Mvc.Areas.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChamadosController : ControllerBase
{
    private readonly ITicketService _tickets;
    public ChamadosController(ITicketService tickets){ _tickets = tickets; }

    public record AbrirChamadoRequest(Guid SolicitanteId, string Titulo, string Descricao);
    public record EnviarMensagemRequest(Guid RemetenteId, string Conteudo, bool Automatica);
    public record MudarStatusRequest(string NovoStatus, Guid? SupportUserId);

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
        => (await _tickets.ListarPorUsuarioAsync(userId)).Select(t => new { t.Id, t.Titulo, Status = t.Status.ToString(), t.CriadoEmUtc, t.EncerradoEmUtc });

    [HttpGet]
    public async Task<IEnumerable<object>> Listar([FromQuery] string? status)
    {
        TicketStatus? s = status != null && Enum.TryParse<TicketStatus>(status, true, out var parsed) ? parsed : null;
        var list = await _tickets.ListarTodosAsync(s);
        return list.Select(t => new { t.Id, t.Titulo, Status = t.Status.ToString(), t.CriadoEmUtc, t.EncerradoEmUtc });
    }

    [HttpGet("{ticketId}")]
    public async Task<ActionResult<object>> Obter(Guid ticketId)
    {
        var t = await _tickets.ObterPorIdAsync(ticketId);
        if (t == null) return NotFound();
        return Ok(new
        {
            t.Id,
            t.Titulo,
            Status = t.Status.ToString(),
            t.CriadoEmUtc,
            t.EncerradoEmUtc,
            Mensagens = t.Mensagens.Select(m => new { m.Id, m.RemetenteUserId, m.Conteudo, m.EnviadoEmUtc, m.Automatica })
        });
    }

    [HttpPost("{ticketId}/mensagens")]
    public async Task<ActionResult<object>> EnviarMensagem(Guid ticketId, [FromBody] EnviarMensagemRequest req)
    {
        var t = await _tickets.EnviarMensagemAsync(ticketId, req.RemetenteId, req.Conteudo, req.Automatica);
        var last = t.Mensagens.Last();
        return Ok(new { last.Id, last.RemetenteUserId, last.Conteudo, last.EnviadoEmUtc, last.Automatica });
    }

    [HttpPost("{ticketId}/status")]
    public async Task<ActionResult<object>> MudarStatus(Guid ticketId, [FromBody] MudarStatusRequest req)
    {
        if (!Enum.TryParse<TicketStatus>(req.NovoStatus, true, out var status)) return Problem("Status inválido", statusCode: 400);
        var t = await _tickets.MudarStatusAsync(ticketId, status, req.SupportUserId);
        return Ok(new { t.Id, t.Titulo, Status = t.Status.ToString(), t.CriadoEmUtc, t.EncerradoEmUtc });
    }

    [HttpPost("{ticketId}/encerrar")]
    public async Task<ActionResult<object>> Encerrar(Guid ticketId)
    {
        var t = await _tickets.EncerrarAsync(ticketId);
        return Ok(new { t.Id, t.Titulo, Status = t.Status.ToString(), t.CriadoEmUtc, t.EncerradoEmUtc });
    }
}
