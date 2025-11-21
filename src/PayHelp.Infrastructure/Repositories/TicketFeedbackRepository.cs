using Microsoft.EntityFrameworkCore;
using PayHelp.Application.Abstractions;
using PayHelp.Domain.Entities;

namespace PayHelp.Infrastructure.Repositories;

public class TicketFeedbackRepository : ITicketFeedbackRepository
{
    private readonly AppDbContext _db;
    public TicketFeedbackRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(TicketFeedback feedback)
    {
        _db.TicketFeedbacks.Add(feedback);
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<TicketFeedback>> GetAllAsync()
        => await _db.TicketFeedbacks.AsNoTracking().ToListAsync();

    public async Task<IEnumerable<TicketFeedback>> GetByTicketAsync(Guid ticketId)
        => await _db.TicketFeedbacks.AsNoTracking().Where(f => f.TicketId == ticketId).OrderByDescending(f => f.DataCriacaoUtc).ToListAsync();

    public async Task<TicketFeedback?> GetByTicketAndUserAsync(Guid ticketId, Guid usuarioId)
        => await _db.TicketFeedbacks.FirstOrDefaultAsync(f => f.TicketId == ticketId && f.UsuarioId == usuarioId);

    public async Task UpdateAsync(TicketFeedback feedback)
    {
        _db.TicketFeedbacks.Update(feedback);
        await _db.SaveChangesAsync();
    }
}
