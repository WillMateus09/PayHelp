using Microsoft.AspNetCore.Mvc;
using PayHelp.Application.Services;

namespace PayHelp.WebApp.Mvc.Areas.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TriagemController : ControllerBase
{
    private readonly ITriageService _triage;
    public TriagemController(ITriageService triage) => _triage = triage;

    public record TriagemRequest(string Texto);

    [HttpPost]
    public async Task<ActionResult<object>> Sugerir([FromBody] TriagemRequest req)
    {
        var texto = await _triage.ObterRespostaAutomaticaAsync(req.Texto);
        return Ok(new { sugestao = texto, origem = "triagem" });
    }
}
