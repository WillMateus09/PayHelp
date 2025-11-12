using Microsoft.Extensions.Logging;
using PayHelp.Application.Abstractions;
using PayHelp.Domain.Entities;

namespace PayHelp.Application.Services;

public class CannedMessageService : ICannedMessageService
{
    private readonly ICannedMessageRepository _repo;
    private readonly ILogger<CannedMessageService> _logger;

    public CannedMessageService(ICannedMessageRepository repo, ILogger<CannedMessageService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public Task<IEnumerable<CannedMessage>> ListarAsync() => _repo.ListAsync();

    public async Task<CannedMessage> CriarAsync(string titulo, string conteudo, string? gatilho = null)
    {
        var msg = new CannedMessage { Titulo = titulo, Conteudo = conteudo, GatilhoPalavraChave = gatilho };
        await _repo.AddAsync(msg);
        _logger.LogInformation("CannedMessage criada: {Titulo}", titulo);
        return msg;
    }

    public async Task RemoverAsync(Guid id)
    {
        await _repo.RemoveAsync(id);
        _logger.LogInformation("CannedMessage removida: {Id}", id);
    }
}
