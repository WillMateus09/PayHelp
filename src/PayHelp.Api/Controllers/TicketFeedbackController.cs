using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayHelp.Application.Abstractions;

namespace PayHelp.Api.Controllers;

[ApiController]
[Route("api/tickets")]
[Authorize]
public class TicketFeedbackController : ControllerBase
{
    private readonly ITicketService _ticketService;
    public TicketFeedbackController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    public record ResolverPeloUsuarioRequest(Guid UsuarioId, int? Nota, string? Feedback);
    public record RegistrarFeedbackRequest(Guid UsuarioId, int? Nota, string? Comentario);

    // POST /api/tickets/{ticketId}/resolver-usuario
    [HttpPost("{ticketId:guid}/resolver-usuario")]
    public async Task<ActionResult<object>> Resolver(Guid ticketId, [FromBody] ResolverPeloUsuarioRequest req)
    {
        try
        {
            var t = await _ticketService.ResolverPeloUsuarioAsync(ticketId, req.UsuarioId, req.Feedback, req.Nota);
            return Ok(new { t.Id, t.Titulo, Status = t.Status.ToString(), t.ResolvidoPeloUsuario, t.FeedbackUsuario, t.NotaUsuario, t.DataResolvidoUsuario });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Problem(ex.Message, statusCode: 403);
        }
        catch (ArgumentException ex)
        {
            return Problem(ex.Message, statusCode: 400);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // POST /api/tickets/{ticketId}/feedback
    [HttpPost("{ticketId:guid}/feedback")]
    public async Task<ActionResult<object>> Registrar(Guid ticketId, [FromBody] RegistrarFeedbackRequest req)
    {
        try
        {
            var fb = await _ticketService.RegistrarFeedbackAsync(ticketId, req.UsuarioId, req.Nota, req.Comentario);
            return Ok(new { fb.Id, fb.TicketId, fb.UsuarioId, fb.Nota, fb.Comentario, fb.DataCriacaoUtc });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Problem(ex.Message, statusCode: 403);
        }
        catch (ArgumentException ex)
        {
            return Problem(ex.Message, statusCode: 400);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (NotSupportedException ex)
        {
            return Problem(ex.Message, statusCode: 501);
        }
    }

    // GET /api/tickets/{ticketId}/feedbacks
    [HttpGet("{ticketId:guid}/feedbacks")]
    public async Task<ActionResult<IEnumerable<object>>> Listar(Guid ticketId)
    {
        var list = await _ticketService.ObterFeedbacksDoTicketAsync(ticketId);
        var shaped = list.Select(f => new { f.Id, f.TicketId, f.UsuarioId, f.Nota, f.Comentario, f.DataCriacaoUtc });
        return Ok(shaped);
    }
}
