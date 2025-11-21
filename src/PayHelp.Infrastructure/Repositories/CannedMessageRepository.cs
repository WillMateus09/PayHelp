using Microsoft.EntityFrameworkCore;
using PayHelp.Application.Abstractions;
using PayHelp.Domain.Entities;

namespace PayHelp.Infrastructure.Repositories;

public class CannedMessageRepository : ICannedMessageRepository
{
    private readonly AppDbContext _db;
    public CannedMessageRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(CannedMessage message)
    {
        _db.CannedMessages.Add(message);
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<CannedMessage>> ListAsync()
        => await _db.CannedMessages.AsNoTracking().OrderBy(c => c.Titulo).ToListAsync();

    public async Task RemoveAsync(Guid id)
    {
        var entity = await _db.CannedMessages.FirstOrDefaultAsync(c => c.Id == id);
        if (entity != null)
        {
            _db.CannedMessages.Remove(entity);
            await _db.SaveChangesAsync();
        }
    }
}