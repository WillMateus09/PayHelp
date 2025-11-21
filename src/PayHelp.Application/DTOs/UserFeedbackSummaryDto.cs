namespace PayHelp.Application.DTOs;

public class UserFeedbackSummaryDto
{
    public Guid UsuarioSuporteId { get; set; }
    public string NomeUsuarioSuporte { get; set; } = string.Empty;
    public int TotalFeedbacks { get; set; }
    public double MediaNotas { get; set; }
    public int TicketsResolvidosPeloUsuario { get; set; }
    public int TicketsResolvidosPeloSuporte { get; set; }
    public double MediaNotasTicketsResolvidosPeloSuporte { get; set; }
    public double MediaNotasTicketsResolvidosPeloUsuario { get; set; }
}
