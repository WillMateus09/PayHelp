using Microsoft.AspNetCore.Mvc;
using PayHelp.Application.Services;

namespace PayHelp.WebApp.Mvc.Areas.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MensagensAutomaticasController : ControllerBase
{
    private readonly ICannedMessageService _svc;
    public MensagensAutomaticasController(ICannedMessageService svc) => _svc = svc;

    public record CreateRequest(string Titulo, string Conteudo);

    [HttpGet]
    public async Task<IEnumerable<object>> Listar()
        => (await _svc.ListarAsync()).Select(m => new { m.Id, m.Titulo, m.Conteudo });

    [HttpPost]
    public async Task<ActionResult<object>> Criar([FromBody] CreateRequest req)
    {
        var msg = await _svc.CriarAsync(req.Titulo, req.Conteudo);
        return Created($"/api/mensagensautomaticas/{msg.Id}", new { msg.Id, msg.Titulo, msg.Conteudo });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remover(Guid id)
    {
        await _svc.RemoverAsync(id);
        return NoContent();
    }
}
