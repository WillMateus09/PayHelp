using PayHelp.Domain.Enums;

namespace PayHelp.Domain.Entities;

public class Ticket
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid? SupportUserId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public TicketStatus Status { get; set; } = TicketStatus.Aberto;
    public DateTime CriadoEmUtc { get; set; } = DateTime.UtcNow;
    public DateTime? EncerradoEmUtc { get; set; }
    public List<TicketMessage> Mensagens { get; set; } = new();
}

public class TicketMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TicketId { get; set; }
    public Guid RemetenteUserId { get; set; }
    public string Conteudo { get; set; } = string.Empty;
    public DateTime EnviadoEmUtc { get; set; } = DateTime.UtcNow;
    public bool Automatica { get; set; }
}

public class CannedMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Titulo { get; set; } = string.Empty;
    public string Conteudo { get; set; } = string.Empty;
    public string? GatilhoPalavraChave { get; set; }
}

public class ReportEntry
{
    public Guid TicketId { get; set; }
    public string SolicitanteEmail { get; set; } = string.Empty;
    public UserRole SolicitanteRole { get; set; }
    public TicketStatus StatusFinal { get; set; }
    public TimeSpan? Duracao { get; set; }
    public DateTime CriadoEmUtc { get; set; }
    public DateTime? EncerradoEmUtc { get; set; }
}
