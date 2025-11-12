using Microsoft.Maui.Controls;
using PayHelp.Mobile.Maui.Helpers;
using PayHelp.Mobile.Maui.ViewModels;

namespace PayHelp.Mobile.Maui.Views;

public partial class DashboardPage : ContentPage
{
    public DashboardPage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetService<DashboardViewModel>() ?? throw new InvalidOperationException("DashboardViewModel not resolved");
    }

    private async void ContentPage_Appearing(object sender, EventArgs e)
    {
        if (BindingContext is DashboardViewModel vm)
            await vm.CarregarAsync();
    }
}
