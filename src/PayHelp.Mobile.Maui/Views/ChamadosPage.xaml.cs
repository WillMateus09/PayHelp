using Microsoft.Maui.Controls;
using PayHelp.Mobile.Maui.Helpers;
using PayHelp.Mobile.Maui.ViewModels;

namespace PayHelp.Mobile.Maui.Views;

public partial class ChamadosPage : ContentPage
{
    public ChamadosPage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetService<ChamadosViewModel>() ?? throw new InvalidOperationException("ChamadosViewModel not resolved");
    }

    private async void ContentPage_Appearing(object sender, EventArgs e)
    {
        if (BindingContext is ChamadosViewModel vm)
            await vm.CarregarAsync();
    }
}
