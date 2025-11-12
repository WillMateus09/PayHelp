using PayHelp.Domain.Enums;

namespace PayHelp.WebApp.Mvc.ViewModels;

public class TicketDetailsViewModel
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public TicketStatus Status { get; set; }


    public DateTime CriadoEmUtc { get; set; }
    public DateTime? EncerradoEmUtc { get; set; }
    public DateTime CriadoEm => (CriadoEmUtc == default ? DateTime.MinValue : CriadoEmUtc.ToLocalTime());
    public DateTime? EncerradoEm => EncerradoEmUtc?.ToLocalTime();


    public List<TicketMessageViewModel> Mensagens { get; set; } = new();


    public Guid CurrentUserId { get; set; }
    public string CurrentUserName { get; set; } = string.Empty;
    public bool CurrentUserIsSupport { get; set; }
}

public class TicketMessageViewModel
{
    public Guid Id { get; set; }
    public Guid RemetenteUserId { get; set; }
    public string Conteudo { get; set; } = string.Empty;
    public DateTime EnviadoEmUtc { get; set; }
    public bool Automatica { get; set; }
}
