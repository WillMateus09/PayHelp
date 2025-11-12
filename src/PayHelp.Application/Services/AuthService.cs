using Microsoft.Extensions.Logging;
using PayHelp.Application.Abstractions;
using PayHelp.Domain.Entities;
using PayHelp.Domain.Enums;
using PayHelp.Domain.Security;

namespace PayHelp.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository users, ILogger<AuthService> logger)
    {
        _users = users;
        _logger = logger;
    }

    public async Task<User> RegistrarUsuarioSimplesAsync(string numero, string nome, string email, string senha)
    {
        await EnsureEmailDisponivelAsync(email);
        var user = new User
        {
            NumeroInscricao = numero,
            Nome = nome,
            Email = email,
            SenhaHash = HashStub.Hash(senha),
            Role = UserRole.Simples
        };
        await _users.AddAsync(user);
        _logger.LogInformation("Usuário simples registrado: {Email}", email);
        return user;
    }

    public async Task<User> RegistrarUsuarioSuporteAsync(string numero, string nome, string email, string senha, string palavraVerificacao)
    {
        if (!string.Equals(palavraVerificacao, "SUP", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Palavra de verificação inválida.");
        }

        await EnsureEmailDisponivelAsync(email);
        var user = new User
        {
            NumeroInscricao = numero,
            Nome = nome,
            Email = email,
            SenhaHash = HashStub.Hash(senha),
            Role = UserRole.Suporte
        };
        await _users.AddAsync(user);
        _logger.LogInformation("Usuário suporte registrado: {Email}", email);
        return user;
    }

    public async Task<User?> LoginAsync(string email, string senha)
    {
        var user = await _users.GetByEmailAsync(email);
        if (user is null) return null;
        var ok = HashStub.Verify(senha, user.SenhaHash);
        if (!ok)
        {


            if (string.Equals(user.SenhaHash, senha, StringComparison.Ordinal))
            {
                user.SenhaHash = HashStub.Hash(senha);
                await _users.UpdateAsync(user);
                ok = true;
            }
        }
        _logger.LogInformation("Login {Result} para {Email}", ok ? "sucesso" : "falhou", email);
        return ok ? user : null;
    }

    private static bool IsSha256Hex(string? s)
    {
        if (string.IsNullOrWhiteSpace(s) || s.Length != 64) return false;
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            bool hex = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
            if (!hex) return false;
        }
        return true;
    }

    private async Task EnsureEmailDisponivelAsync(string email)
    {
        var existing = await _users.GetByEmailAsync(email);
        if (existing != null)
            throw new InvalidOperationException("E-mail já cadastrado.");
    }
}
