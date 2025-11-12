using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PayHelp.Mobile.Maui.Models;
using PayHelp.Mobile.Maui.Services;

namespace PayHelp.Mobile.Maui.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly RelatorioService _relatorios;

    [ObservableProperty] private DateTime? dataInicio;
    [ObservableProperty] private DateTime? dataFim;
    [ObservableProperty] private string? status;

    [ObservableProperty] private RelatorioResumo? resumo;

    public DashboardViewModel(RelatorioService relatorios)
    {
        _relatorios = relatorios;
    }

    [RelayCommand]
    public async Task CarregarAsync()
    {
        try
        {
            Resumo = await _relatorios.GerarAsync(new RelatorioRequest
            {
                DataInicio = DataInicio,
                DataFim = DataFim,
                Status = Status
            });
        }
        catch (Exception ex)
        {
            await AlertAsync("Erro", ex.Message);
        }
    }
}
