using PayHelp.Mobile.Maui.ViewModels;

namespace PayHelp.Mobile.Maui.Views;

public partial class AdminSettingsPage : ContentPage
{
    public AdminSettingsPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var vm = BindingContext as AdminSettingsViewModel;
        if (vm != null)
        {
            await vm.LoadSettingsCommand.ExecuteAsync(null);
        }
    }
}
