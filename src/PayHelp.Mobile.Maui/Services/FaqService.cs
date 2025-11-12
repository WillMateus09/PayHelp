using System.Net.Http.Json;

namespace PayHelp.Mobile.Maui.Services;

public class FaqService
{
    private readonly IHttpClientFactory _factory;
    public FaqService(IHttpClientFactory factory) { _factory = factory; }


    public record FaqItem(int Id, string? TituloProblema, string? Solucao, DateTime DataCriacao, Guid? TicketId);

    public async Task<List<FaqItem>> BuscarAsync(string texto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(texto)) return new();
        var client = _factory.CreateClient("api");
        var res = await client.PostAsJsonAsync("faq/buscar", new { Texto = texto }, ct);
        if (!res.IsSuccessStatusCode) return new();
        var list = await res.Content.ReadFromJsonAsync<List<FaqItem>>(cancellationToken: ct);
        return list ?? new();
    }
}
