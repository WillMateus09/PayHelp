using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayHelp.Application.Abstractions;

namespace PayHelp.Api.Controllers;

[ApiController]
[Route("api/feedback")]
[Authorize(Roles = "Suporte")]
public class FeedbackAgregadoController : ControllerBase
{
    private readonly ITicketService _ticketService;
    public FeedbackAgregadoController(ITicketService svc) { _ticketService = svc; }

    // GET /api/feedback/usuarios
    [HttpGet("usuarios")]
    public async Task<ActionResult<IEnumerable<object>>> PorUsuarios()
    {
        var data = await _ticketService.ObterFeedbackUsuariosAsync();
        return Ok(data);
    }
}
