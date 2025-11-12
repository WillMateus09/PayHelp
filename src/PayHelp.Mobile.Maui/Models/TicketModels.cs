namespace PayHelp.Mobile.Maui.Models;

public class TicketCreateRequest
{
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
}

public class TicketMessageRequest
{
    public string Texto { get; set; } = string.Empty;
}

public class TicketChangeStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public class TicketDto
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DataAbertura { get; set; }
    public Guid UsuarioId { get; set; }
    public DateTime? EncerradoEmUtc { get; set; }
    public bool Triaging { get; set; }
    public string? ResolucaoFinal { get; set; }
    public List<TicketMessageDto> Mensagens { get; set; } = new();
}

public class TicketMessageDto
{
    public Guid Id { get; set; }
    public string Texto { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public Guid AutorId { get; set; }
    public string? AutorNome { get; set; }
}
