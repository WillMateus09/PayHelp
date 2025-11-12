using System.Net.Http.Json;
using PayHelp.Mobile.Maui.Models;

namespace PayHelp.Mobile.Maui.Services;

public class TriagemService
{
    private readonly IHttpClientFactory _factory;

    public TriagemService(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    public async Task<TriagemResponse?> SugerirAsync(string texto, CancellationToken ct = default)
    {
        var client = _factory.CreateClient("api");
        var res = await client.PostAsJsonAsync("triagem", new TriagemRequest { Texto = texto }, ct);
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<TriagemResponse>(cancellationToken: ct);
    }
}
