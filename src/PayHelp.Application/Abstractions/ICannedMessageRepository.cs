using PayHelp.Domain.Entities;

namespace PayHelp.Application.Abstractions;

public interface ICannedMessageRepository
{
    Task<IEnumerable<CannedMessage>> ListAsync();
    Task AddAsync(CannedMessage message);
    Task RemoveAsync(Guid id);
}
