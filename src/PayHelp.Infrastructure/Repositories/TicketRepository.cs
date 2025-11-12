using Microsoft.EntityFrameworkCore;
using PayHelp.Application.Abstractions;
using PayHelp.Domain.Entities;
using PayHelp.Domain.Enums;

namespace PayHelp.Infrastructure.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly AppDbContext _db;
    public TicketRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(Ticket ticket)
    {
        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync();
    }

    public async Task<Ticket?> GetByIdAsync(Guid id)
        => await _db.Tickets.Include(t => t.Mensagens).FirstOrDefaultAsync(t => t.Id == id);

    public async Task<IEnumerable<Ticket>> ListAllAsync(TicketStatus? status = null)
    {
        IQueryable<Ticket> q = _db.Tickets.Include(t => t.Mensagens).OrderByDescending(t => t.CriadoEmUtc);
        if (status.HasValue) q = q.Where(t => t.Status == status);
        return await q.ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> ListByUserAsync(Guid userId)
        => await _db.Tickets.Where(t => t.UserId == userId).OrderByDescending(t => t.CriadoEmUtc).ToListAsync();

    public async Task UpdateAsync(Ticket ticket)
    {
        _db.Tickets.Update(ticket);
        await _db.SaveChangesAsync();
    }

    public async Task AddMessageAsync(TicketMessage message)
    {
        _db.TicketMessages.Add(message);
        await _db.SaveChangesAsync();
    }
}
