using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PayHelp.Mobile.Maui.Models;
using PayHelp.Mobile.Maui.Services;
using Microsoft.Maui.Storage;

namespace PayHelp.Mobile.Maui.ViewModels;

public partial class ChamadosViewModel : BaseViewModel
{
    private readonly ChamadoService _chamados;

    [ObservableProperty] private List<TicketDto> itens = new();
    [ObservableProperty] private string? filtroStatus;
    [ObservableProperty] private bool isSuporte;
    [ObservableProperty] private bool isNaoSuporte;
    [ObservableProperty] private bool somenteAbertos;

    public ChamadosViewModel(ChamadoService chamados)
    {
        _chamados = chamados;
    }

    [RelayCommand]
    public async Task CarregarAsync()
    {
        if (IsBusy) return; IsBusy = true;
        try
        {
            var role = (await SecureStorage.Default.GetAsync("user_role")) ?? string.Empty;
            IsSuporte = role.ToLowerInvariant().Contains("suporte") || role.ToLowerInvariant().Contains("support");
            IsNaoSuporte = !IsSuporte;

            if (IsSuporte)
            {
                var status = SomenteAbertos ? "Aberto" : FiltroStatus;
                Itens = await _chamados.ListAllAsync(status);
            }
            else
            {
                var userIdStr = await SecureStorage.Default.GetAsync("user_id");
                if (Guid.TryParse(userIdStr, out var userId))
                {
                    var lista = await _chamados.ListByUserAsync(userId);
                    Itens = SomenteAbertos
                        ? lista.Where(x => string.Equals(x.Status, "Aberto", StringComparison.OrdinalIgnoreCase)).ToList()
                        : lista;
                }
            }
        }
        catch (Exception ex)
        {
            await AlertAsync("Erro", ex.Message);
        }
        finally { IsBusy = false; }
    }

    partial void OnIsSuporteChanged(bool value)
    {
        IsNaoSuporte = !value;
    }

    [RelayCommand]
    public async Task AbrirChamadoAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("abrir-chamado");
        }
        catch (Exception ex)
        {
            await AlertAsync("Navegação", $"Falha ao abrir página: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task AbrirDetalheAsync(TicketDto? ticket)
    {
        if (ticket is null) return;
        try
        {
            var parms = new Dictionary<string, object> { { "ticketId", ticket.Id } };
            await Shell.Current.GoToAsync("chamado-detalhe", parms);
        }
        catch (Exception ex)
        {
            await AlertAsync("Navegação", $"Falha ao abrir detalhes: {ex.Message}");
        }
    }
}
