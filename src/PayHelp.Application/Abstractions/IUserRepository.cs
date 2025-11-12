using PayHelp.Domain.Entities;

namespace PayHelp.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task<IEnumerable<User>> GetAllAsync();
}
