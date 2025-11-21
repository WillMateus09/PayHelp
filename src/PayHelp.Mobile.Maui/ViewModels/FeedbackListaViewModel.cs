using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PayHelp.Mobile.Maui.Models;
using PayHelp.Mobile.Maui.Services;

namespace PayHelp.Mobile.Maui.ViewModels;

public partial class FeedbackListaViewModel : BaseViewModel
{
    private readonly RelatorioService _relatorios;

    [ObservableProperty] private List<FeedbackCompletoDto> feedbacks = new();

    public FeedbackListaViewModel(RelatorioService relatorios)
    {
        _relatorios = relatorios;
    }

    [RelayCommand]
    public async Task CarregarAsync()
    {
        IsBusy = true;
        try
        {
            Feedbacks = await _relatorios.ListarFeedbacksCompletosAsync();
        }
        catch (Exception ex)
        {
            await AlertAsync("Erro", $"Falha ao carregar feedbacks: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
