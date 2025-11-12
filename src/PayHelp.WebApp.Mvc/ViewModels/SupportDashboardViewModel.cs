using PayHelp.Domain.Enums;

namespace PayHelp.WebApp.Mvc.ViewModels;

public class SupportDashboardViewModel
{
    public IEnumerable<TicketListItemViewModel> Abertos { get; set; } = Enumerable.Empty<TicketListItemViewModel>();
    public IEnumerable<TicketListItemViewModel> EmAtendimento { get; set; } = Enumerable.Empty<TicketListItemViewModel>();
    public IEnumerable<TicketListItemViewModel> Encerrados { get; set; } = Enumerable.Empty<TicketListItemViewModel>();
    public TicketStatus? FiltroSelecionado { get; set; }
    public ISet<Guid> TriagingIds { get; set; } = new HashSet<Guid>();
}
