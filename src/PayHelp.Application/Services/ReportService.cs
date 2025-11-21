using PayHelp.Application.Abstractions;
using PayHelp.Domain.Entities;
using PayHelp.Domain.Enums;

namespace PayHelp.Application.Services;

public class ReportService : IReportService
{
    private readonly IReportSink _sink;
    private readonly ITicketRepository _tickets;
    private readonly IUserRepository _users;

    public ReportService(IReportSink sink, ITicketRepository tickets, IUserRepository users)
    {
        _sink = sink;
        _tickets = tickets;
        _users = users;
    }

    public async Task<IEnumerable<ReportEntry>> GerarRelatorioAsync(DateTime? de = null, DateTime? ate = null, TicketStatus? status = null)
    {
        // Carrega entradas encerradas do sink (se houver) e sincroniza com o estado atual do ticket
        var closedEntries = (await _sink.ListAsync()).ToList();

        // Carrega todos os tickets para enriquecer/sintetizar entradas
        var allTicketsForClosed = await _tickets.ListAllAsync(null);
        // Carrega usuários para preencher email e role do solicitante
        var allUsers = (await _users.GetAllAsync()).ToList();
        var usersById = allUsers.ToDictionary(u => u.Id, u => u);
        var byId = allTicketsForClosed.ToDictionary(t => t.Id, t => t);
        foreach (var e in closedEntries)
        {
            if (byId.TryGetValue(e.TicketId, out var t))
            {
                e.ResolvidoPeloUsuario = t.ResolvidoPeloUsuario;
                e.FeedbackUsuario = t.FeedbackUsuario;
                e.NotaUsuario = t.NotaUsuario;
                if (usersById.TryGetValue(t.UserId, out var u))
                {
                    e.SolicitanteEmail = u.Email ?? string.Empty;
                    e.SolicitanteRole = u.Role;
                }
            }
        }

        // Se o sink não tiver entradas para alguns tickets já encerrados,
        // sintetiza-as a partir dos próprios tickets para garantir que
        // feedbacks antigos apareçam no relatório.
        var closedTicketSynth = allTicketsForClosed
            .Where(t => t.Status == TicketStatus.Encerrado || (t.Status == TicketStatus.ResolvidoPeloUsuario && t.EncerradoEmUtc.HasValue))
            .ToList();

        var existingClosedIds = closedEntries.Select(e => e.TicketId).ToHashSet();
        foreach (var t in closedTicketSynth)
        {
            if (existingClosedIds.Contains(t.Id)) continue;
            var u = usersById.TryGetValue(t.UserId, out var usr) ? usr : null;
            closedEntries.Add(new ReportEntry
            {
                TicketId = t.Id,
                SolicitanteEmail = u?.Email ?? string.Empty,
                SolicitanteRole = u?.Role ?? Domain.Enums.UserRole.Simples,
                StatusFinal = t.Status, // Mantém o status original (Encerrado ou ResolvidoPeloUsuario)
                CriadoEmUtc = t.CriadoEmUtc,
                EncerradoEmUtc = t.EncerradoEmUtc,
                Duracao = (t.EncerradoEmUtc.HasValue ? t.EncerradoEmUtc.Value - t.CriadoEmUtc : null),
                ResolvidoPeloUsuario = t.ResolvidoPeloUsuario,
                FeedbackUsuario = t.FeedbackUsuario,
                NotaUsuario = t.NotaUsuario
            });
        }

        IEnumerable<ReportEntry> closed = closedEntries;


        DateTime? deUtc = de?.ToUniversalTime();
        DateTime? ateUtc = ate?.ToUniversalTime();

        // Para entradas encerradas, filtrar por EncerradoEmUtc quando disponível;
        // caso contrário, usar a data de criação como fallback.
        if (deUtc.HasValue) closed = closed.Where(r => (r.EncerradoEmUtc ?? r.CriadoEmUtc) >= deUtc.Value);
        if (ateUtc.HasValue) closed = closed.Where(r => (r.EncerradoEmUtc ?? r.CriadoEmUtc) <= ateUtc.Value);

        if (status.HasValue)
        {
            if (status.Value == TicketStatus.Encerrado)
            {
                return closed.Where(r => r.StatusFinal == TicketStatus.Encerrado).ToList();
            }
            if (status.Value == TicketStatus.ResolvidoPeloUsuario)
            {
                // Filtra por resoluções de autoatendimento (via IA/usuário)
                return closed.Where(r => r.ResolvidoPeloUsuario).ToList();
            }
        }


        var allTickets = allTicketsForClosed; // já carregados acima
        IEnumerable<Ticket> openTickets = allTickets
            .Where(t => t.Status != TicketStatus.Encerrado && !(t.Status == TicketStatus.ResolvidoPeloUsuario && t.EncerradoEmUtc.HasValue));

        if (deUtc.HasValue) openTickets = openTickets.Where(t => t.CriadoEmUtc >= deUtc.Value);
        if (ateUtc.HasValue) openTickets = openTickets.Where(t => t.CriadoEmUtc <= ateUtc.Value);
        if (status.HasValue)
        {

            openTickets = openTickets.Where(t => t.Status == status.Value);
        }


        var openAsEntries = openTickets.Select(t => new ReportEntry
        {
            TicketId = t.Id,
            SolicitanteEmail = (usersById.TryGetValue(t.UserId, out var usr) ? usr.Email : string.Empty) ?? string.Empty,
            SolicitanteRole = usersById.TryGetValue(t.UserId, out var usr2) ? usr2.Role : Domain.Enums.UserRole.Simples,
            StatusFinal = t.Status,
            CriadoEmUtc = t.CriadoEmUtc,
            EncerradoEmUtc = null,
            Duracao = null,
            ResolvidoPeloUsuario = t.ResolvidoPeloUsuario,
            FeedbackUsuario = t.FeedbackUsuario,
            NotaUsuario = t.NotaUsuario
        });


        IEnumerable<ReportEntry> result;
        if (status.HasValue && status.Value != TicketStatus.Encerrado)
        {
            if (status.Value == TicketStatus.ResolvidoPeloUsuario)
            {
                result = closed.Where(r => r.ResolvidoPeloUsuario).Concat(openAsEntries.Where(e => e.ResolvidoPeloUsuario));
            }
            else
            {
                result = closed.Where(r => r.StatusFinal == status.Value).Concat(openAsEntries);
            }
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
