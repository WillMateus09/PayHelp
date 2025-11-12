using System.Collections.Concurrent;
using PayHelp.Application.Abstractions;
using PayHelp.Domain.Entities;
using PayHelp.Domain.Enums;

namespace PayHelp.Infrastructure.InMemory;

public class TicketRepository : ITicketRepository
{
    private readonly ConcurrentDictionary<Guid, Ticket> _tickets = new();

    public Task AddAsync(Ticket ticket)
    {
        _tickets[ticket.Id] = ticket;
        return Task.CompletedTask;
    }

    public Task<Ticket?> GetByIdAsync(Guid id)
    {
        _tickets.TryGetValue(id, out var t);
        return Task.FromResult<Ticket?>(t);
    }

    public Task<IEnumerable<Ticket>> ListAllAsync(TicketStatus? status = null)
    {
        IEnumerable<Ticket> res = _tickets.Values.OrderByDescending(t => t.CriadoEmUtc);
        if (status.HasValue) res = res.Where(t => t.Status == status);
        return Task.FromResult(res);
    }

    public Task<IEnumerable<Ticket>> ListByUserAsync(Guid userId)
    {
        IEnumerable<Ticket> res = _tickets.Values.Where(t => t.UserId == userId).OrderByDescending(t => t.CriadoEmUtc);
        return Task.FromResult(res);
    }

    public Task UpdateAsync(Ticket ticket)
    {
        _tickets[ticket.Id] = ticket;
        return Task.CompletedTask;
    }

    public Task AddMessageAsync(TicketMessage message)
    {
        if (_tickets.TryGetValue(message.TicketId, out var t))
        {
            t.Mensagens.Add(message);
        }
        return Task.CompletedTask;
    }
}
