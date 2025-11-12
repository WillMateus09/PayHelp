using Microsoft.Extensions.Logging;
using PayHelp.Application.Abstractions;

namespace PayHelp.Application.Services;

public class TriageService : ITriageService
{
    private readonly ICannedMessageRepository _canned;
    private readonly ILogger<TriageService> _logger;

    public TriageService(ICannedMessageRepository canned, ILogger<TriageService> logger)
    {
        _canned = canned;
        _logger = logger;
    }

    public async Task<string> ObterRespostaAutomaticaAsync(string mensagemUsuario)
    {
        if (string.IsNullOrWhiteSpace(mensagemUsuario))
            return "Não encontrei uma solução automática. Deseja chamar um atendente?";

        var lista = await _canned.ListAsync();
        var lower = mensagemUsuario.ToLowerInvariant();
        foreach (var item in lista)
        {
            if (!string.IsNullOrWhiteSpace(item.GatilhoPalavraChave))
            {
                var gatilhos = item.GatilhoPalavraChave.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (gatilhos.Any(g => lower.Contains(g.ToLowerInvariant())))
                {
                    _logger.LogInformation("Triagem encontrou resposta automática: {Titulo}", item.Titulo);
                    return item.Conteudo;
                }
            }
        }
        return "Não encontrei uma solução automática. Deseja chamar um atendente?";
    }
}
