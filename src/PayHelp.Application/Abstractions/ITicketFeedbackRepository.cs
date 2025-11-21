using PayHelp.Domain.Entities;

namespace PayHelp.Application.Abstractions;

public interface ITicketFeedbackRepository
{
    Task AddAsync(TicketFeedback feedback);
    Task UpdateAsync(TicketFeedback feedback);
    Task<IEnumerable<TicketFeedback>> GetByTicketAsync(Guid ticketId);
    Task<IEnumerable<TicketFeedback>> GetAllAsync();
    Task<TicketFeedback?> GetByTicketAndUserAsync(Guid ticketId, Guid usuarioId);
}
