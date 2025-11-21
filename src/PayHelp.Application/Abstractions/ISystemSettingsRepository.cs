using PayHelp.Domain.Entities;

namespace PayHelp.Application.Abstractions;

public interface ISystemSettingsRepository
{
    Task<SystemSetting?> GetAsync(string key);
    Task SetAsync(string key, string? value);
}
