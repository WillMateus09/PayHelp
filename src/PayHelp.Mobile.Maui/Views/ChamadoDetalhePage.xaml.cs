using Microsoft.Maui.Controls;
using PayHelp.Mobile.Maui.Helpers;
using PayHelp.Mobile.Maui.ViewModels;

namespace PayHelp.Mobile.Maui.Views;

public partial class ChamadoDetalhePage : ContentPage
{
    public ChamadoDetalhePage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetService<ChamadoDetalheViewModel>() ?? throw new InvalidOperationException("ChamadoDetalheViewModel not resolved");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            if (BindingContext is ChamadoDetalheViewModel vm)
                await vm.CarregarAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha ao carregar detalhes: {ex.Message}", "OK");
        }
    }
}
