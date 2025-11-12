using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PayHelp.Mobile.Maui.Models;
using PayHelp.Mobile.Maui.Services;

namespace PayHelp.Mobile.Maui.ViewModels;

public partial class AbrirChamadoViewModel : BaseViewModel
{
    private readonly ChamadoService _chamados;
    private readonly TriagemService _triagem;
    private readonly FaqService _faq;

    [ObservableProperty] private string titulo = string.Empty;
    [ObservableProperty] private string descricao = string.Empty;
    [ObservableProperty] private List<string> sugestoes = new();

    public AbrirChamadoViewModel(ChamadoService chamados, TriagemService triagem, FaqService faq)
    {
        _chamados = chamados; _triagem = triagem; _faq = faq;
    }

    partial void OnDescricaoChanged(string value)
    {


        Sugestoes = new();
    }

    // Método mantido apenas para referência; não é mais chamado nesta tela.
    private Task BuscarSugestoesAsync(string texto) => Task.CompletedTask;

    [RelayCommand]
    private async Task EnviarAsync()
    {
        if (IsBusy) return; IsBusy = true;
        try
        {
            var created = await _chamados.CreateAsync(new TicketCreateRequest { Titulo = Titulo, Descricao = Descricao });
            if (created == null) { await AlertAsync("Erro", "Não foi possível abrir o chamado."); return; }

            try
            {
                var faq = await _faq.BuscarAsync(Descricao);
                if (faq.Count > 0 && !string.IsNullOrWhiteSpace(faq[0].Solucao))
                {
                    var textoFaq = $"FAQ: {faq[0].Solucao!.Trim()}";
                    await _chamados.SendMessageAsync(created.Id, new TicketMessageRequest { Texto = textoFaq }, automatic: true);
                }
                else
                {
                    await _chamados.SendMessageAsync(created.Id, new TicketMessageRequest { Texto = "Não encontramos uma solução para seu problema no banco de resoluções antigas (FAQ). Em seguida, utilize a Triagem IA para buscar soluções documentadas pelo suporte." }, automatic: true);
                }
            }
            catch {  }


            var parms = new Dictionary<string, object> { { "ticketId", created.Id } };
            await Shell.Current.GoToAsync("chamado-detalhe", parms);
        }
        catch (Exception ex) { await AlertAsync("Erro", ex.Message); }
        finally { IsBusy = false; }
    }
}
