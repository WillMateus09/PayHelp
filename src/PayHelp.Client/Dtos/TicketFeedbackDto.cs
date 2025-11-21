namespace PayHelp.Client.Dtos;

public sealed class TicketFeedbackDto
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid UsuarioId { get; set; }
    public int? Nota { get; set; }
    public string? Comentario { get; set; }
    public DateTime DataCriacaoUtc { get; set; }
}
