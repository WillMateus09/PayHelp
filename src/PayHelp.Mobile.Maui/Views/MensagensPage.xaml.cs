using Microsoft.Maui.Controls;
using PayHelp.Mobile.Maui.Helpers;
using PayHelp.Mobile.Maui.ViewModels;

namespace PayHelp.Mobile.Maui.Views;

public partial class MensagensPage : ContentPage
{
    public MensagensPage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetService<MensagensViewModel>() ?? throw new InvalidOperationException("MensagensViewModel not resolved");
    }

    private async void ContentPage_Appearing(object sender, EventArgs e)
    {
        if (BindingContext is MensagensViewModel vm)
            await vm.CarregarAsync();
    }
}
