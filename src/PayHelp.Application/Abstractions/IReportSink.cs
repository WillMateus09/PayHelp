using PayHelp.Domain.Entities;

namespace PayHelp.Application.Abstractions;

public interface IReportSink
{
    Task AddAsync(ReportEntry entry);
    Task<IEnumerable<ReportEntry>> ListAsync();
}
