using Microsoft.Extensions.Logging;
using PayHelp.Application.Abstractions;
using PayHelp.Domain.Entities;
using PayHelp.Domain.Enums;

namespace PayHelp.Application.Services;







public class TicketService : ITicketService
{
    private readonly ITicketRepository _tickets;
    private readonly IUserRepository _users;
    private readonly IReportSink _reportSink;
    private readonly ILogger<TicketService> _logger;

    public TicketService(ITicketRepository tickets, IUserRepository users, IReportSink reportSink, ILogger<TicketService> logger)
    {
        _tickets = tickets;
        _users = users;
        _reportSink = reportSink;
        _logger = logger;
    }

    public async Task<Ticket> AbrirChamadoAsync(Guid solicitanteId, string titulo, string descricao)
    {
        if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Título e Descrição são obrigatórios.");

        var ticket = new Ticket
        {
            UserId = solicitanteId,
            Titulo = titulo,
            Descricao = descricao,
            Status = TicketStatus.Aberto,
            CriadoEmUtc = DateTime.UtcNow
        };
        ticket.Mensagens.Add(new TicketMessage
        {
            TicketId = ticket.Id,
            RemetenteUserId = solicitanteId,
            Conteudo = descricao,
            EnviadoEmUtc = DateTime.UtcNow,
            Automatica = false
        });

        await _tickets.AddAsync(ticket);
        _logger.LogInformation("Ticket aberto: {TicketId} por {UserId}", ticket.Id, solicitanteId);
        return ticket;
    }

    public Task<IEnumerable<Ticket>> ListarPorUsuarioAsync(Guid solicitanteId) => _tickets.ListByUserAsync(solicitanteId);

    public Task<IEnumerable<Ticket>> ListarTodosAsync(TicketStatus? filtro) => _tickets.ListAllAsync(filtro);

    public Task<Ticket?> ObterPorIdAsync(Guid ticketId) => _tickets.GetByIdAsync(ticketId);

    public async Task<Ticket> EnviarMensagemAsync(Guid ticketId, Guid remetenteId, string conteudo, bool automatica = false)
    {
        var ticket = await _tickets.GetByIdAsync(ticketId) ?? throw new KeyNotFoundException("Ticket não encontrado.");

        conteudo = conteudo?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(conteudo))
            throw new ArgumentException("Conteúdo da mensagem é obrigatório.");
        if (automatica && remetenteId == Guid.Empty)
        {

            remetenteId = ticket.UserId;
        }

        if (remetenteId != Guid.Empty)
        {
            var sender = await _users.GetByIdAsync(remetenteId);
            if (sender == null)
                throw new KeyNotFoundException("Usuário remetente não encontrado.");
        }
        var msg = new TicketMessage
        {
            TicketId = ticketId,
            RemetenteUserId = remetenteId,
            Conteudo = conteudo,
            EnviadoEmUtc = DateTime.UtcNow,
            Automatica = automatica
        };

        await _tickets.AddMessageAsync(msg);

        ticket.Mensagens.Add(msg);
        _logger.LogInformation("Mensagem adicionada ao ticket {TicketId}", ticketId);
        return ticket;
    }

    public async Task<Ticket> MudarStatusAsync(Guid ticketId, TicketStatus novoStatus, Guid? supportUserId = null)
    {
        var ticket = await _tickets.GetByIdAsync(ticketId) ?? throw new KeyNotFoundException("Ticket não encontrado.");
        if (novoStatus == TicketStatus.EmAtendimento)
        {


            if (ticket.Status == TicketStatus.Encerrado)
                throw new InvalidOperationException("Não é possível assumir um chamado encerrado.");
            ticket.SupportUserId = supportUserId;
        }
        else if (novoStatus == TicketStatus.Encerrado)
        {

            if (ticket.Status == TicketStatus.Encerrado)
                return ticket;
        }
        ticket.Status = novoStatus;
        await _tickets.UpdateAsync(ticket);
        _logger.LogInformation("Status do ticket {TicketId} alterado para {Status}", ticket.Id, ticket.Status);
        return ticket;
    }

    public async Task<Ticket> EncerrarAsync(Guid ticketId)
    {
        var ticket = await _tickets.GetByIdAsync(ticketId) ?? throw new KeyNotFoundException("Ticket não encontrado.");
        if (ticket.Status == TicketStatus.Encerrado)
            return ticket;

        ticket.Status = TicketStatus.Encerrado;
        ticket.EncerradoEmUtc = DateTime.UtcNow;
        await _tickets.UpdateAsync(ticket);

        var solicitante = await _users.GetByIdAsync(ticket.UserId);
        var entry = new ReportEntry
        {
            TicketId = ticket.Id,
            SolicitanteEmail = solicitante?.Email ?? "desconhecido",
            SolicitanteRole = solicitante?.Role ?? UserRole.Simples,
            StatusFinal = ticket.Status,
            CriadoEmUtc = ticket.CriadoEmUtc,
            EncerradoEmUtc = ticket.EncerradoEmUtc,
            Duracao = ticket.EncerradoEmUtc.HasValue ? ticket.EncerradoEmUtc.Value - ticket.CriadoEmUtc : null
        };
        await _reportSink.AddAsync(entry);

        _logger.LogInformation("Ticket encerrado {TicketId}", ticket.Id);
        return ticket;
    }
}
