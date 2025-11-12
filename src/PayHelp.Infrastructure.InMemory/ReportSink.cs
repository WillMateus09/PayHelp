using System.Collections.Concurrent;
using PayHelp.Application.Abstractions;
using PayHelp.Domain.Entities;

namespace PayHelp.Infrastructure.InMemory;

public class ReportSink : IReportSink
{
    private readonly ConcurrentBag<ReportEntry> _entries = new();

    public Task AddAsync(ReportEntry entry)
    {
        _entries.Add(entry);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<ReportEntry>> ListAsync() => Task.FromResult<IEnumerable<ReportEntry>>(_entries.ToArray());
}
