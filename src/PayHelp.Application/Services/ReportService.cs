using PayHelp.Application.Abstractions;
using PayHelp.Domain.Entities;
using PayHelp.Domain.Enums;

namespace PayHelp.Application.Services;

public class ReportService : IReportService
{
    private readonly IReportSink _sink;
    private readonly ITicketRepository _tickets;

    public ReportService(IReportSink sink, ITicketRepository tickets)
    {
        _sink = sink;
        _tickets = tickets;
    }

    public async Task<IEnumerable<ReportEntry>> GerarRelatorioAsync(DateTime? de = null, DateTime? ate = null, TicketStatus? status = null)
    {

        var closedEntries = await _sink.ListAsync();
        var closed = closedEntries.AsQueryable();


        DateTime? deUtc = de?.ToUniversalTime();
        DateTime? ateUtc = ate?.ToUniversalTime();

        if (deUtc.HasValue) closed = closed.Where(r => r.CriadoEmUtc >= deUtc.Value);
        if (ateUtc.HasValue) closed = closed.Where(r => r.CriadoEmUtc <= ateUtc.Value);

        if (status.HasValue)
        {

            if (status.Value == TicketStatus.Encerrado)
            {
                return closed.Where(r => r.StatusFinal == TicketStatus.Encerrado).ToList();
            }
        }


        var allTickets = await _tickets.ListAllAsync(null);
        var openTickets = allTickets
            .Where(t => t.Status != TicketStatus.Encerrado)
            .AsQueryable();

        if (deUtc.HasValue) openTickets = openTickets.Where(t => t.CriadoEmUtc >= deUtc.Value);
        if (ateUtc.HasValue) openTickets = openTickets.Where(t => t.CriadoEmUtc <= ateUtc.Value);
        if (status.HasValue)
        {

            openTickets = openTickets.Where(t => t.Status == status.Value);
        }


        var openAsEntries = openTickets.Select(t => new ReportEntry
        {
            TicketId = t.Id,
            SolicitanteEmail = string.Empty,
            SolicitanteRole = Domain.Enums.UserRole.Simples,
            StatusFinal = t.Status,
            CriadoEmUtc = t.CriadoEmUtc,
            EncerradoEmUtc = null,
            Duracao = null
        });


        IEnumerable<ReportEntry> result;
        if (status.HasValue && status.Value != TicketStatus.Encerrado)
        {
            result = closed.Where(r => r.StatusFinal == status.Value).Concat(openAsEntries);
        }
        else if (status.HasValue && status.Value == TicketStatus.Encerrado)
        {
            result = closed.Where(r => r.StatusFinal == TicketStatus.Encerrado);
        }
        else
        {
            // Sem filtro: incluir todos encerrados + todos abertos
            result = closed.Concat(openAsEntries);
        }

        return result.ToList();
    }
}
