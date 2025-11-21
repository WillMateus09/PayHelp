using System.Threading.Tasks;

namespace PayHelp.Client;

public interface ITokenStore
{
    Task SetAsync(string token);
    Task<string?> GetAsync();
    Task ClearAsync();
}
