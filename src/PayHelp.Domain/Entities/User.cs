using PayHelp.Domain.Enums;

namespace PayHelp.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string NumeroInscricao { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Simples;
}
