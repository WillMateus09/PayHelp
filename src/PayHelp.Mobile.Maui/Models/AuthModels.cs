using System.Text.Json.Serialization;

namespace PayHelp.Mobile.Maui.Models;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string NumeroInscricao { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;

    public string? PalavraVerificacao { get; set; }
}

public class LoginResponse
{
    public Guid UserId { get; set; }
    public string? Nome { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public string? Token { get; set; }
    public string? AccessToken { get; set; }

    [JsonIgnore]
    public string EffectiveToken => !string.IsNullOrWhiteSpace(Token) ? Token! : (AccessToken ?? string.Empty);
}
