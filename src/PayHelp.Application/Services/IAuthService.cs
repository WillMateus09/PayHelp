using PayHelp.Domain.Entities;

namespace PayHelp.Application.Services;

public interface IAuthService
{
    Task<User> RegistrarUsuarioSimplesAsync(string numero, string nome, string email, string senha);
    Task<User> RegistrarUsuarioSuporteAsync(string numero, string nome, string email, string senha, string palavraVerificacao);
    Task<User?> LoginAsync(string email, string senha);
}
