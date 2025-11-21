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
    private readonly ITicketFeedbackRepository? _feedbacks;

    public TicketService(ITicketRepository tickets, IUserRepository users, IReportSink reportSink, ILogger<TicketService> logger, ITicketFeedbackRepository? feedbacks = null)
    {
        _tickets = tickets;
        _users = users;
        _reportSink = reportSink;
        _logger = logger;
        _feedbacks = feedbacks; // opcional em soluções antigas
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

        // Preserva ResolvidoPeloUsuario se já estava nesse status
        if (ticket.Status != TicketStatus.ResolvidoPeloUsuario)
        {
            ticket.Status = TicketStatus.Encerrado;
        }
        
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
            Duracao = ticket.EncerradoEmUtc.HasValue ? ticket.EncerradoEmUtc.Value - ticket.CriadoEmUtc : null,
            ResolvidoPeloUsuario = ticket.ResolvidoPeloUsuario,
            FeedbackUsuario = ticket.FeedbackUsuario,
            NotaUsuario = ticket.NotaUsuario
        };
        await _reportSink.AddAsync(entry);

        _logger.LogInformation("Ticket encerrado {TicketId}", ticket.Id);
        return ticket;
    }

    public async Task<Ticket> MarcarComoResolvidoPeloUsuarioAsync(Guid ticketId, string feedbackUsuario, int notaUsuario)
    {
        var ticket = await _tickets.GetByIdAsync(ticketId) ?? throw new KeyNotFoundException("Ticket não encontrado.");
        
        // Permite marcar como resolvido sem feedback (será adicionado depois)
        if (notaUsuario > 0)
        {
            if (notaUsuario < 1 || notaUsuario > 5) throw new ArgumentException("A nota deve estar entre 1 e 5 estrelas.");
            ticket.NotaUsuario = notaUsuario;
        }
        
        if (!string.IsNullOrWhiteSpace(feedbackUsuario))
        {
            ticket.FeedbackUsuario = feedbackUsuario.Trim();
        }
        
        if (ticket.Status != TicketStatus.Encerrado)
        {
            ticket.Status = TicketStatus.ResolvidoPeloUsuario;
            ticket.EncerradoEmUtc ??= DateTime.UtcNow;
        }
        
        ticket.ResolvidoPeloUsuario = true;
        ticket.DataResolvidoUsuario = DateTime.UtcNow;
        await _tickets.UpdateAsync(ticket);
        
        // Registra também na coleção de feedbacks quando disponível e houver nota
        if (_feedbacks != null && notaUsuario > 0)
        {
            await RegistrarFeedbackAsync(ticketId, ticket.UserId, notaUsuario, feedbackUsuario);
        }
        
        _logger.LogInformation("Ticket {TicketId} marcado como resolvido pelo usuário com nota {Nota}", ticketId, notaUsuario);
        return ticket;
    }

    // Novo método: formalização conforme contrato atualizado
    public async Task<Ticket> ResolverPeloUsuarioAsync(Guid ticketId, Guid usuarioId, string? feedback, int? nota)
    {
        var ticket = await _tickets.GetByIdAsync(ticketId) ?? throw new KeyNotFoundException("Ticket não encontrado.");
        if (usuarioId == Guid.Empty) throw new ArgumentException("usuarioId inválido.");
        if (ticket.UserId != usuarioId) throw new UnauthorizedAccessException("Usuário não autorizado a resolver este ticket.");
        if (nota.HasValue && (nota < 1 || nota > 5)) throw new ArgumentException("Nota deve ser entre 1 e 5.");

        // Se ainda não resolvido, marca como resolvido pelo usuário
        if (ticket.Status != TicketStatus.Encerrado && !ticket.ResolvidoPeloUsuario)
        {
            ticket.Status = TicketStatus.ResolvidoPeloUsuario;
            ticket.ResolvidoPeloUsuario = true;
            ticket.DataResolvidoUsuario = DateTime.UtcNow;
            ticket.EncerradoEmUtc ??= DateTime.UtcNow;
        }

        // Persistir feedback básico no Ticket para compatibilidade
        if (!string.IsNullOrWhiteSpace(feedback)) ticket.FeedbackUsuario = feedback.Trim();
        if (nota.HasValue) ticket.NotaUsuario = nota.Value;
        await _tickets.UpdateAsync(ticket);

        // Registrar feedback separado (permite avaliação tardia)
        if (_feedbacks != null)
        {
            await RegistrarFeedbackAsync(ticketId, usuarioId, nota, feedback);
        }
        return ticket;
    }

    public async Task<TicketFeedback> RegistrarFeedbackAsync(Guid ticketId, Guid usuarioId, int? nota, string? comentario)
    {
        if (_feedbacks == null) throw new NotSupportedException("Repositório de feedback não está configurado nesta execução.");
        var ticket = await _tickets.GetByIdAsync(ticketId) ?? throw new KeyNotFoundException("Ticket não encontrado.");
        if (ticket.UserId != usuarioId) throw new UnauthorizedAccessException("Usuário não autorizado a avaliar este ticket.");
        if (nota.HasValue && (nota < 1 || nota > 5)) throw new ArgumentException("Nota deve ser entre 1 e 5.");

        var existing = await _feedbacks.GetByTicketAndUserAsync(ticketId, usuarioId);
        if (existing is null)
        {
            var fb = new TicketFeedback
            {
                TicketId = ticketId,
                UsuarioId = usuarioId,
                Nota = nota,
                Comentario = string.IsNullOrWhiteSpace(comentario) ? null : comentario.Trim(),
                DataCriacaoUtc = DateTime.UtcNow
            };
            await _feedbacks.AddAsync(fb);
            return fb;
        }
        else
        {
            existing.Nota = nota;
            existing.Comentario = string.IsNullOrWhiteSpace(comentario) ? existing.Comentario : comentario.Trim();
            await _feedbacks.UpdateAsync(existing);
            return existing;
        }
    }

    public async Task<IEnumerable<TicketFeedback>> ObterFeedbacksDoTicketAsync(Guid ticketId)
    {
        if (_feedbacks == null) return Enumerable.Empty<TicketFeedback>();
        return await _feedbacks.GetByTicketAsync(ticketId);
    }

    public async Task<IEnumerable<PayHelp.Application.DTOs.UserFeedbackSummaryDto>> ObterFeedbackUsuariosAsync()
    {
        // Carrega tickets e feedbacks
        // Utiliza o repositório para listar todos os tickets (método correto: ListAllAsync)
        var tickets = await _tickets.ListAllAsync(null);
        var feedbacks = _feedbacks != null ? await _feedbacks.GetAllAsync() : Enumerable.Empty<TicketFeedback>();
        var usersById = new Dictionary<Guid, PayHelp.Domain.Entities.User>();
        // Tenta materializar nomes de suporte
        foreach (var uid in tickets.Where(t => t.SupportUserId.HasValue).Select(t => t.SupportUserId!.Value).Distinct())
        {
            var u = await _users.GetByIdAsync(uid);
            if (u != null) usersById[uid] = u;
        }

        var bySupport = tickets.GroupBy(t => t.SupportUserId ?? Guid.Empty);
        var result = new List<PayHelp.Application.DTOs.UserFeedbackSummaryDto>();
        foreach (var grp in bySupport)
        {
            var supId = grp.Key;
            var name = usersById.TryGetValue(supId, out var u) ? (u.Nome ?? u.Email) : (supId == Guid.Empty ? "(Sem atendente)" : "(Desconhecido)");
            var grpTicketIds = grp.Select(t => t.Id).ToHashSet();
            var fbs = feedbacks.Where(f => grpTicketIds.Contains(f.TicketId)).ToList();

            double mediaGeral = fbs.Any(f => f.Nota.HasValue) ? fbs.Where(f => f.Nota.HasValue).Average(f => f.Nota!.Value) : 0;
            var resolvidosUsuario = grp.Count(t => t.ResolvidoPeloUsuario);
            var resolvidosSuporte = grp.Count(t => !t.ResolvidoPeloUsuario && t.Status == TicketStatus.Encerrado);

            var idsUsuario = grp.Where(t => t.ResolvidoPeloUsuario).Select(t => t.Id).ToHashSet();
            var idsSuporte = grp.Where(t => !t.ResolvidoPeloUsuario && t.Status == TicketStatus.Encerrado).Select(t => t.Id).ToHashSet();
            var notasUsuario = fbs.Where(f => f.Nota.HasValue && idsUsuario.Contains(f.TicketId)).Select(f => f.Nota!.Value).ToList();
            var notasSuporte = fbs.Where(f => f.Nota.HasValue && idsSuporte.Contains(f.TicketId)).Select(f => f.Nota!.Value).ToList();

            result.Add(new PayHelp.Application.DTOs.UserFeedbackSummaryDto
            {
                UsuarioSuporteId = supId,
                NomeUsuarioSuporte = name,
                TotalFeedbacks = fbs.Count,
                MediaNotas = mediaGeral,
                TicketsResolvidosPeloUsuario = resolvidosUsuario,
                TicketsResolvidosPeloSuporte = resolvidosSuporte,
                MediaNotasTicketsResolvidosPeloSuporte = notasSuporte.Count > 0 ? notasSuporte.Average() : 0,
                MediaNotasTicketsResolvidosPeloUsuario = notasUsuario.Count > 0 ? notasUsuario.Average() : 0
            });
        }
        return result.OrderByDescending(r => r.MediaNotas).ToList();
    }
}
