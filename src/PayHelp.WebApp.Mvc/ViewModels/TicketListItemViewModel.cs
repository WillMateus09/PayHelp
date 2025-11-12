using PayHelp.Domain.Enums;

namespace PayHelp.WebApp.Mvc.ViewModels;

public class TicketListItemViewModel
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public TicketStatus Status { get; set; }


    public DateTime CriadoEmUtc { get; set; }
    public DateTime? EncerradoEmUtc { get; set; }


    public bool PossuiResolucao { get; set; }
    public string? ResumoResolucao { get; set; }


    public DateTime CriadoEm => CriadoEmUtc;
    public DateTime? EncerradoEm => EncerradoEmUtc;
}
