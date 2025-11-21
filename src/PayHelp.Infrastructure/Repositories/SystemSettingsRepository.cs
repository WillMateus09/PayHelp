using Microsoft.EntityFrameworkCore;
using PayHelp.Application.Abstractions;
using PayHelp.Domain.Entities;

namespace PayHelp.Infrastructure.Repositories;

public class SystemSettingsRepository : ISystemSettingsRepository
{
    private readonly AppDbContext _db;
    public SystemSettingsRepository(AppDbContext db) => _db = db;

    public async Task<SystemSetting?> GetAsync(string key)
        => await _db.SystemSettings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == key);

    public async Task SetAsync(string key, string? value)
    {
        var existing = await _db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (existing == null)
        {
            existing = new SystemSetting { Key = key, Value = value, UpdatedAtUtc = DateTime.UtcNow };
            _db.SystemSettings.Add(existing);
        }
        else
        {
            existing.Value = value;
            existing.UpdatedAtUtc = DateTime.UtcNow;
            _db.SystemSettings.Update(existing);
        }
        await _db.SaveChangesAsync();
    }
}
