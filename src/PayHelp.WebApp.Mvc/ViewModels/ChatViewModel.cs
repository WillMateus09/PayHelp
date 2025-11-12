using PayHelp.Domain.Enums;

namespace PayHelp.WebApp.Mvc.ViewModels;

public class ChatViewModel
{
    public Guid TicketId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public TicketStatus Status { get; set; }
    public List<ChatMessageVM> Mensagens { get; set; } = new();
    public string? NovaMensagem { get; set; }
}

public class ChatMessageVM
{
    public Guid RemetenteUserId { get; set; }
    public string RemetenteNome { get; set; } = string.Empty;
    public string Conteudo { get; set; } = string.Empty;
    public DateTime EnviadoEm { get; set; }
    public bool Automatica { get; set; }
}
