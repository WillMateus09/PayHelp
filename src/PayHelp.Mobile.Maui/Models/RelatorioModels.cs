namespace PayHelp.Mobile.Maui.Models;

public class RelatorioRequest
{
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public string? Status { get; set; }
}


public class RelatorioItem
{
    public Guid TicketId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? SolicitanteEmail { get; set; }
    public string? SolicitanteRole { get; set; }
    public TimeSpan? Duracao { get; set; }
    public DateTime CriadoEmUtc { get; set; }
    public DateTime? EncerradoEmUtc { get; set; }
    public bool ResolvidoPeloUsuario { get; set; }
    public string? FeedbackUsuario { get; set; }
    public int? NotaUsuario { get; set; }
}

public class RelatorioResumo
{
    public int Total { get; set; }
    public int Abertos { get; set; }
    public int Encerrados { get; set; }
    public int EmAtendimento { get; set; }
    public int ChamadosResolvidosIA { get; set; }
    public double TempoMedioResolucaoHoras { get; set; }
    public double TaxaResolucaoPercent { get; set; }
}
