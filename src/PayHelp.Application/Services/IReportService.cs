using PayHelp.Domain.Entities;
using PayHelp.Domain.Enums;

namespace PayHelp.Application.Services;

public interface IReportService
{
    Task<IEnumerable<ReportEntry>> GerarRelatorioAsync(DateTime? de = null, DateTime? ate = null, TicketStatus? status = null);
}
