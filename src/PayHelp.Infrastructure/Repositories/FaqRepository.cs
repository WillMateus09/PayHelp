using Microsoft.EntityFrameworkCore;
using PayHelp.Application.Abstractions;
using PayHelp.Domain.Entities;

namespace PayHelp.Infrastructure.Repositories;

public class FaqRepository : IFaqRepository
{
    private readonly AppDbContext _db;
    public FaqRepository(AppDbContext db) => _db = db;

    public async Task<FaqEntry> AddAsync(FaqEntry entry, CancellationToken ct = default)
    {
        _db.FaqEntries.Add(entry);
        await _db.SaveChangesAsync(ct);
        return entry;
    }

    public async Task<IEnumerable<FaqEntry>> ListAllAsync(CancellationToken ct = default)
        => await _db.FaqEntries.AsNoTracking().OrderByDescending(f => f.DataCriacao).ToListAsync(ct);

    public async Task<IEnumerable<FaqEntry>> SearchAsync(string text, int max = 10, CancellationToken ct = default)
    {
        text = text ?? string.Empty;

        var pattern = $"%{text.ToLower()}%";
        return await _db.FaqEntries
            .Where(f => EF.Functions.Like(f.DescricaoProblema.ToLower(), pattern) || EF.Functions.Like(f.TituloProblema.ToLower(), pattern))
            .OrderByDescending(f => f.DataCriacao)
            .Take(max)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public Task<bool> ExistsForTicketAsync(Guid ticketId, CancellationToken ct = default)
        => _db.FaqEntries.AnyAsync(f => f.TicketId == ticketId, ct);

    public Task<FaqEntry?> GetByTicketAsync(Guid ticketId, CancellationToken ct = default)
        => _db.FaqEntries.AsNoTracking().FirstOrDefaultAsync(f => f.TicketId == ticketId, ct);
}