namespace PayHelp.Mobile.Maui.Models;

public class FeedbackDto
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid UsuarioId { get; set; }
    public string? UsuarioNome { get; set; }
    public string? TicketTitulo { get; set; }
    public int? NotaUsuario { get; set; }
    public string? FeedbackUsuario { get; set; }
    public DateTime? DataResolvidoUsuario { get; set; }
}

public class FeedbackCompletoDto
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string TicketTitulo { get; set; } = string.Empty;
    public Guid UsuarioId { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public string UsuarioEmail { get; set; } = string.Empty;
    public int? Nota { get; set; }
    public string? Comentario { get; set; }
    public DateTime? DataCriacaoUtc { get; set; }
    
    // Propriedades computadas
    public string NotaTexto => Nota switch
    {
        1 => "⭐ (1/5)",
        2 => "⭐⭐ (2/5)",
        3 => "⭐⭐⭐ (3/5)",
        4 => "⭐⭐⭐⭐ (4/5)",
        5 => "⭐⭐⭐⭐⭐ (5/5)",
        _ => "Sem avaliação"
    };
    
    public DateTime DataCriacao => DataCriacaoUtc?.ToLocalTime() ?? DateTime.Now;
}

public class FeedbackEstatisticaDto
{
    public int TotalAvaliacoes { get; set; }
    public double MediaNotas { get; set; }
    public int Nota1 { get; set; }
    public int Nota2 { get; set; }
    public int Nota3 { get; set; }
    public int Nota4 { get; set; }
    public int Nota5 { get; set; }
}
