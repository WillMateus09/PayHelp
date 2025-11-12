using System.Net.Http.Json;
using PayHelp.Mobile.Maui.Models;

namespace PayHelp.Mobile.Maui.Services;

public class MensagemService
{
    private readonly IHttpClientFactory _factory;

    public MensagemService(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    public async Task<List<MensagemAutomaticaDto>> ListAsync(CancellationToken ct = default)
    {
        var client = _factory.CreateClient("api");
        return await client.GetFromJsonAsync<List<MensagemAutomaticaDto>>("mensagensautomaticas", ct) ?? new();
    }

    public async Task<bool> CreateAsync(MensagemAutomaticaCreateRequest req, CancellationToken ct = default)
    {
        var client = _factory.CreateClient("api");
        var res = await client.PostAsJsonAsync("mensagensautomaticas", req, ct);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var client = _factory.CreateClient("api");
        var res = await client.DeleteAsync($"mensagensautomaticas/{id}", ct);
        return res.IsSuccessStatusCode;
    }
}
