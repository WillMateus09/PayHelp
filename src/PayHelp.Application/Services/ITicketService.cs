using PayHelp.Domain.Entities;
using PayHelp.Domain.Enums;

namespace PayHelp.Application.Services;

public interface ITicketService
{
    Task<Ticket> AbrirChamadoAsync(Guid solicitanteId, string titulo, string descricao);
    Task<IEnumerable<Ticket>> ListarPorUsuarioAsync(Guid solicitanteId);
    Task<IEnumerable<Ticket>> ListarTodosAsync(TicketStatus? filtro);
    Task<Ticket?> ObterPorIdAsync(Guid ticketId);
    Task<Ticket> EnviarMensagemAsync(Guid ticketId, Guid remetenteId, string conteudo, bool automatica = false);
    Task<Ticket> MudarStatusAsync(Guid ticketId, TicketStatus novoStatus, Guid? supportUserId = null);
    Task<Ticket> EncerrarAsync(Guid ticketId);
}
