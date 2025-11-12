using PayHelp.Domain.Entities;

namespace PayHelp.Application.Abstractions;

public interface IFaqRepository
{
    Task<FaqEntry> AddAsync(FaqEntry entry, CancellationToken ct = default);
    Task<IEnumerable<FaqEntry>> ListAllAsync(CancellationToken ct = default);
    Task<IEnumerable<FaqEntry>> SearchAsync(string text, int max = 10, CancellationToken ct = default);
    Task<bool> ExistsForTicketAsync(Guid ticketId, CancellationToken ct = default);
    Task<FaqEntry?> GetByTicketAsync(Guid ticketId, CancellationToken ct = default);
}
