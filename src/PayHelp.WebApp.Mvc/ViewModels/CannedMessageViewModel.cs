namespace PayHelp.WebApp.Mvc.ViewModels;

public class CannedMessageViewModel
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Conteudo { get; set; } = string.Empty;
    public string? GatilhoPalavraChave { get; set; }
    public string? Categoria { get; set; }
    public string? Problema { get; set; }
}
