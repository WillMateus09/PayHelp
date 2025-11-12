namespace PayHelp.Application.Services;

public interface ITriageService
{
    Task<string> ObterRespostaAutomaticaAsync(string mensagemUsuario);
}
