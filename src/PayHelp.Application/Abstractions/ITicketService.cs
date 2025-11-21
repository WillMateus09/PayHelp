using PayHelp.Domain.Entities;
using PayHelp.Domain.Enums;

namespace PayHelp.Application.Abstractions;

public interface ITicketService
{
    Task<Ticket> AbrirChamadoAsync(Guid solicitanteId, string titulo, string descricao);
    Task<IEnumerable<Ticket>> ListarPorUsuarioAsync(Guid solicitanteId);
    Task<IEnumerable<Ticket>> ListarTodosAsync(TicketStatus? filtro);
    Task<Ticket?> ObterPorIdAsync(Guid ticketId);
    Task<Ticket> EnviarMensagemAsync(Guid ticketId, Guid remetenteId, string conteudo, bool automatica = false);
    Task<Ticket> MudarStatusAsync(Guid ticketId, TicketStatus novoStatus, Guid? supportUserId = null);
    Task<Ticket> EncerrarAsync(Guid ticketId);
    
    // Método legado (mantido p/ compatibilidade)
    Task<Ticket> MarcarComoResolvidoPeloUsuarioAsync(Guid ticketId, string feedbackUsuario, int notaUsuario);

    // Novos métodos formais
    Task<Ticket> ResolverPeloUsuarioAsync(Guid ticketId, Guid usuarioId, string? feedback, int? nota);
    Task<TicketFeedback> RegistrarFeedbackAsync(Guid ticketId, Guid usuarioId, int? nota, string? comentario);
    Task<IEnumerable<TicketFeedback>> ObterFeedbacksDoTicketAsync(Guid ticketId);
    Task<IEnumerable<PayHelp.Application.DTOs.UserFeedbackSummaryDto>> ObterFeedbackUsuariosAsync();
}
