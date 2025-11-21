using System.Net.Http.Json;
using PayHelp.Mobile.Maui.Models;

namespace PayHelp.Mobile.Maui.Services;

public class RelatorioService
{
    private readonly IHttpClientFactory _factory;

    public RelatorioService(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    public async Task<RelatorioResumo?> GerarAsync(RelatorioRequest req, CancellationToken ct = default)
    {
        var client = _factory.CreateClient("api");
        var res = await client.PostAsJsonAsync("relatorios", new
        {

            DeUtc = req.DataInicio,
            AteUtc = req.DataFim,
            Status = req.Status
        }, ct);
        if (!res.IsSuccessStatusCode) return null;


        var itens = await res.Content.ReadFromJsonAsync<List<RelatorioItem>>(cancellationToken: ct) 
                    ?? new List<RelatorioItem>();

    var total = itens.Count;
    var encerrados = itens.Count(i => string.Equals(i.Status, "Encerrado", StringComparison.OrdinalIgnoreCase));
    var emAtendimento = itens.Count(i => string.Equals(i.Status, "EmAtendimento", StringComparison.OrdinalIgnoreCase) || string.Equals(i.Status, "Em Atendimento", StringComparison.OrdinalIgnoreCase));
    var abertos = total - (encerrados + emAtendimento);
    var resolvidosIA = itens.Count(i => i.ResolvidoPeloUsuario);
        double mediaHoras = 0.0;
        var resolvidosComDuracao = itens.Where(i => i.Duracao.HasValue).Select(i => i.Duracao!.Value).ToList();
        if (resolvidosComDuracao.Count > 0)
        {
            mediaHoras = resolvidosComDuracao.Average(ts => ts.TotalHours);
        }

        double taxaResolucao = total > 0 ? (double)encerrados / total * 100.0 : 0.0;

        return new RelatorioResumo
        {
            Total = total,
            Abertos = abertos,
            Encerrados = encerrados,
            EmAtendimento = emAtendimento,
            ChamadosResolvidosIA = resolvidosIA,
            TempoMedioResolucaoHoras = mediaHoras,
            TaxaResolucaoPercent = taxaResolucao
        };
    }

    public async Task<List<FeedbackCompletoDto>> ListarFeedbacksCompletosAsync(CancellationToken ct = default)
    {
        try
        {
            var client = _factory.CreateClient("api");
            var response = await client.GetAsync("chamados/feedbacks", ct);
            
            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"[RelatorioService] Erro ao buscar feedbacks: {response.StatusCode}");
                return new List<FeedbackCompletoDto>();
            }

            var feedbacks = await response.Content.ReadFromJsonAsync<List<FeedbackCompletoDto>>(cancellationToken: ct)
                           ?? new List<FeedbackCompletoDto>();
            
            System.Diagnostics.Debug.WriteLine($"[RelatorioService] Feedbacks recebidos: {feedbacks.Count}");
            return feedbacks;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RelatorioService] Exceção ao buscar feedbacks: {ex.Message}");
            return new List<FeedbackCompletoDto>();
        }
    }
}
