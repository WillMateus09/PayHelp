using PayHelp.Domain.Entities;
using PayHelp.Domain.Enums;

namespace PayHelp.Application.Abstractions;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(Guid id);
    Task AddAsync(Ticket ticket);
    Task UpdateAsync(Ticket ticket);

    Task AddMessageAsync(TicketMessage message);
    Task<IEnumerable<Ticket>> ListByUserAsync(Guid userId);
    Task<IEnumerable<Ticket>> ListAllAsync(TicketStatus? status = null);
}
