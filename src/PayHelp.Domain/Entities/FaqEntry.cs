namespace PayHelp.Domain.Entities;

public class FaqEntry
{
    public int Id { get; set; }
    public string TituloProblema { get; set; } = string.Empty;
    public string DescricaoProblema { get; set; } = string.Empty;
    public string Solucao { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public Guid? TicketId { get; set; }
}
