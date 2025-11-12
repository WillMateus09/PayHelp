using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayHelp.Application.Services;
using PayHelp.Domain.Enums;

namespace PayHelp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Suporte")]
public class RelatoriosController : ControllerBase
{
    private readonly IReportService _reports;
    public RelatoriosController(IReportService reports) => _reports = reports;

    public record Filtro(DateTime? DeUtc, DateTime? AteUtc, string? Status);

    [HttpPost]
    public async Task<IEnumerable<object>> Gerar([FromBody] Filtro f)
    {
        TicketStatus? s = f.Status != null && Enum.TryParse<TicketStatus>(f.Status, true, out var parsed) ? parsed : null;
        var items = await _reports.GerarRelatorioAsync(f.DeUtc, f.AteUtc, s);
        return items.Select(r => new 
        {
            r.TicketId,
            Status = r.StatusFinal.ToString(),
            r.SolicitanteEmail,
            SolicitanteRole = r.SolicitanteRole.ToString(),
            r.Duracao,
            r.CriadoEmUtc,
            r.EncerradoEmUtc
        });
    }
}
