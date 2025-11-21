namespace PayHelp.Domain.Entities;

public class TicketFeedback
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TicketId { get; set; }
    public Guid UsuarioId { get; set; }
    public int? Nota { get; set; } // 1-5 (ou null)
    public string? Comentario { get; set; }
    public DateTime DataCriacaoUtc { get; set; } = DateTime.UtcNow;
}
