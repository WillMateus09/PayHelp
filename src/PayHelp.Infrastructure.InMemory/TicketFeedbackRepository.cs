using System.Collections.Concurrent;
using PayHelp.Application.Abstractions;
using PayHelp.Domain.Entities;

namespace PayHelp.Infrastructure.InMemory;

public class TicketFeedbackRepository : ITicketFeedbackRepository
{
    private readonly ConcurrentDictionary<Guid, TicketFeedback> _store = new();

    public Task AddAsync(TicketFeedback feedback)
    {
        _store[feedback.Id] = feedback;
        return Task.CompletedTask;
    }

    public Task<IEnumerable<TicketFeedback>> GetAllAsync()
        => Task.FromResult<IEnumerable<TicketFeedback>>(_store.Values.ToList());

    public Task<IEnumerable<TicketFeedback>> GetByTicketAsync(Guid ticketId)
        => Task.FromResult<IEnumerable<TicketFeedback>>(_store.Values.Where(f => f.TicketId == ticketId).OrderByDescending(f => f.DataCriacaoUtc).ToList());

    public Task<TicketFeedback?> GetByTicketAndUserAsync(Guid ticketId, Guid usuarioId)
        => Task.FromResult(_store.Values.FirstOrDefault(f => f.TicketId == ticketId && f.UsuarioId == usuarioId));

    public Task UpdateAsync(TicketFeedback feedback)
    {
        _store[feedback.Id] = feedback;
        return Task.CompletedTask;
    }
}
