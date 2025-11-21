namespace PayHelp.Client.Dtos;

public sealed class TicketDto
{
    public Guid Id { get; set; }
    public string? Titulo { get; set; }
    public string? Status { get; set; }
    public Guid? UsuarioId { get; set; }
    public string? UltimaMensagem { get; set; }
    public DateTime? CriadoEm { get; set; }
    public DateTime? DataResolvidoUsuario { get; set; }
    public int? NotaUsuario { get; set; }
    public string? FeedbackUsuario { get; set; }
}
