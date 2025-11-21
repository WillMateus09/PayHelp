using Microsoft.EntityFrameworkCore;
using PayHelp.Application.Abstractions;
using PayHelp.Domain.Entities;

namespace PayHelp.Infrastructure.Repositories;

public class ReportSink : IReportSink
{
    private readonly AppDbContext _db;
    public ReportSink(AppDbContext db) => _db = db;

    public async Task AddAsync(ReportEntry entry)
    {



        _db.Add(entry);
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<ReportEntry>> ListAsync()
    {

        return await _db.Set<ReportEntry>().AsNoTracking().OrderByDescending(r => r.CriadoEmUtc).ToListAsync();
    }
}