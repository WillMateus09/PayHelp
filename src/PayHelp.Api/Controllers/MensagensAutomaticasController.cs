using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayHelp.Application.Services;

namespace PayHelp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Suporte")]
public class MensagensAutomaticasController : ControllerBase
{
    private readonly ICannedMessageService _svc;
    public MensagensAutomaticasController(ICannedMessageService svc) => _svc = svc;

    public record CreateRequest(string Titulo, string Conteudo, string? GatilhoPalavraChave);

    [HttpGet]
    public async Task<IEnumerable<object>> Listar()
        => (await _svc.ListarAsync()).Select(m => new { m.Id, m.Titulo, m.Conteudo, m.GatilhoPalavraChave });

    [HttpPost]
    public async Task<ActionResult<object>> Criar([FromBody] CreateRequest req)
    {
        var msg = await _svc.CriarAsync(req.Titulo, req.Conteudo, req.GatilhoPalavraChave);
        return Created($"/api/mensagensautomaticas/{msg.Id}", new { msg.Id, msg.Titulo, msg.Conteudo, msg.GatilhoPalavraChave });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remover(Guid id)
    {
        await _svc.RemoverAsync(id);
        return NoContent();
    }
}
