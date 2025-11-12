using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PayHelp.Application.Abstractions;

namespace PayHelp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FaqController : ControllerBase
{
    private readonly IFaqService _faq;
    public FaqController(IFaqService faq) => _faq = faq;

    public record BuscarRequest(string Texto);

    [HttpPost("buscar")] 
    public async Task<ActionResult<object>> Buscar([FromBody] BuscarRequest req)
    {
        var itens = await _faq.BuscarSimilarAsync(req.Texto);
        return Ok(itens.Select(i => new { i.Id, i.TituloProblema, i.Solucao, i.DataCriacao, i.TicketId }));
    }
}
