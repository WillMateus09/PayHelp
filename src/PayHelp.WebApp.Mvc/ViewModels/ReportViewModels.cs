using PayHelp.Domain.Enums;

namespace PayHelp.WebApp.Mvc.ViewModels;

public class ReportFilterViewModel
{
    public DateTime? De { get; set; }
    public DateTime? Ate { get; set; }
    public TicketStatus? Status { get; set; }
}

public class ReportEntryViewModel
{
    public Guid TicketId { get; set; }
    public string SolicitanteEmail { get; set; } = string.Empty;
    public TicketStatus StatusFinal { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? EncerradoEm { get; set; }
    public TimeSpan? Duracao { get; set; }
}
