namespace PayHelp.Client.Dtos;

public sealed class AuthRequest
{
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public sealed class AuthResponse
{
    // Ajustar conforme resposta real da API
    public string? Token { get; set; }
    public string? AccessToken { get; set; }
    public string? Access_Token { get; set; }
}

public sealed class ResolveTicketRequest
{
    public Guid UsuarioId { get; set; }
    public string? Feedback { get; set; }
    public int? Nota { get; set; }
}
