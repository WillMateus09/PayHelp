namespace PayHelp.Domain.DTOs;

/// <summary>
/// DTO para capturar feedback e nota do usuário ao marcar chamado como resolvido
/// </summary>
public class UserResolutionDto
{
    public string FeedbackUsuario { get; set; } = string.Empty;
    public int NotaUsuario { get; set; }  // 1 a 5 estrelas
}

/// <summary>
/// DTO de resposta após marcar como resolvido
/// </summary>
public class UserResolutionResponseDto
{
    public Guid TicketId { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool ResolvidoPeloUsuario { get; set; }
    public string? FeedbackUsuario { get; set; }
    public int? NotaUsuario { get; set; }
    public DateTime? DataResolvidoUsuario { get; set; }
    public string Mensagem { get; set; } = string.Empty;
}
