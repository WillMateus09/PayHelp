using PayHelp.Domain.Entities;

namespace PayHelp.Application.Services;

public interface ICannedMessageService
{
    Task<IEnumerable<CannedMessage>> ListarAsync();
    Task<CannedMessage> CriarAsync(string titulo, string conteudo, string? gatilho = null);
    Task RemoverAsync(Guid id);
}
