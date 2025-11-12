using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayHelp.Application.Services;
using PayHelp.Application.Abstractions;

namespace PayHelp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TriagemController : ControllerBase
{
    private readonly ITriageService _triage;
    private readonly IFaqService _faq;
    public TriagemController(ITriageService triage, IFaqService faq) { _triage = triage; _faq = faq; }

    public record TriagemRequest(string Texto);

    [HttpPost]
    public async Task<ActionResult<object>> Sugerir([FromBody] TriagemRequest req)
    {
        var respostaAutomatica = await _triage.ObterRespostaAutomaticaAsync(req.Texto);
        var faqMatches = await _faq.BuscarSimilarAsync(req.Texto);
        var faqSugestao = faqMatches.FirstOrDefault();
        string? faqTexto = null;
        if (faqSugestao != null)
        {
            faqTexto = $"Encontrei algo parecido: {faqSugestao.TituloProblema} -> {faqSugestao.Solucao}";
        }
        return Ok(new { sugestao = respostaAutomatica, origem = "triagem", faq = faqTexto });
    }
}
