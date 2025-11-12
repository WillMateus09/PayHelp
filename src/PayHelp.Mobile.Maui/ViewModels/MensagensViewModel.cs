using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PayHelp.Mobile.Maui.Models;
using PayHelp.Mobile.Maui.Services;

namespace PayHelp.Mobile.Maui.ViewModels;

public partial class MensagensViewModel : BaseViewModel
{
    private readonly MensagemService _mensagens;

    [ObservableProperty] private List<MensagemAutomaticaDto> itens = new();
    [ObservableProperty] private string novoTitulo = string.Empty;
    [ObservableProperty] private string novoConteudo = string.Empty;
    [ObservableProperty] private string? novosGatilhos;

    public MensagensViewModel(MensagemService mensagens) { _mensagens = mensagens; }

    [RelayCommand]
    public async Task CarregarAsync()
    {
        Itens = await _mensagens.ListAsync();
    }

    [RelayCommand]
    public async Task CriarAsync()
    {
        if (string.IsNullOrWhiteSpace(NovoTitulo) || string.IsNullOrWhiteSpace(NovoConteudo)) return;
        var req = new MensagemAutomaticaCreateRequest { Titulo = NovoTitulo, Conteudo = NovoConteudo, GatilhoPalavraChave = NovosGatilhos };
        if (await _mensagens.CreateAsync(req))
        {
            NovoTitulo = string.Empty;
            NovoConteudo = string.Empty;
            NovosGatilhos = string.Empty;
            await CarregarAsync();
        }
        else await AlertAsync("Erro", "Não foi possível criar a mensagem.");
    }

    [RelayCommand]
    public async Task RemoverAsync(MensagemAutomaticaDto? msg)
    {
        if (msg == null) return;
        if (await _mensagens.DeleteAsync(msg.Id))
            await CarregarAsync();
    }
}
