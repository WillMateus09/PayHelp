using System.Collections.Concurrent;
using PayHelp.Application.Abstractions;
using PayHelp.Domain.Entities;

namespace PayHelp.Infrastructure.InMemory;

public class UserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<Guid, User> _users = new();

    public Task AddAsync(User user)
    {
        _users[user.Id] = user;
        return Task.CompletedTask;
    }

    public Task<IEnumerable<User>> GetAllAsync() => Task.FromResult<IEnumerable<User>>(_users.Values);

    public Task<User?> GetByEmailAsync(string email)
    {
        var u = _users.Values.FirstOrDefault(x => string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult<User?>(u);
    }

    public Task<User?> GetByIdAsync(Guid id)
    {
        _users.TryGetValue(id, out var u);
        return Task.FromResult<User?>(u);
    }

    public Task UpdateAsync(User user)
    {
        _users[user.Id] = user;
        return Task.CompletedTask;
    }
}
