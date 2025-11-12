using System.Collections.Concurrent;
using PayHelp.Application.Abstractions;
using PayHelp.Domain.Entities;

namespace PayHelp.Infrastructure.InMemory;

public class CannedMessageRepository : ICannedMessageRepository
{
    private readonly ConcurrentDictionary<Guid, CannedMessage> _store = new();

    public Task AddAsync(CannedMessage message)
    {
        _store[message.Id] = message;
        return Task.CompletedTask;
    }

    public Task<IEnumerable<CannedMessage>> ListAsync() => Task.FromResult<IEnumerable<CannedMessage>>(_store.Values.OrderBy(x => x.Titulo));

    public Task RemoveAsync(Guid id)
    {
        _store.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
