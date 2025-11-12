namespace PayHelp.Mobile.Maui.Models;

public class MensagemAutomaticaDto
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Conteudo { get; set; } = string.Empty;
    public string? GatilhoPalavraChave { get; set; }
}

public class MensagemAutomaticaCreateRequest
{
    public string Titulo { get; set; } = string.Empty;
    public string Conteudo { get; set; } = string.Empty;
    public string? GatilhoPalavraChave { get; set; }
}
