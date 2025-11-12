namespace PayHelp.Mobile.Maui.Models;

public class TriagemRequest
{
    public string Texto { get; set; } = string.Empty;
}

public class TriagemResponse
{

    public string? Sugestao { get; set; }
    public string? Origem { get; set; }
    public string? Faq { get; set; }
}
