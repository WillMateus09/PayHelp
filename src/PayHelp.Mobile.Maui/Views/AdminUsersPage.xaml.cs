using PayHelp.Mobile.Maui.ViewModels;

namespace PayHelp.Mobile.Maui.Views;

public partial class AdminUsersPage : ContentPage
{
    public AdminUsersPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var vm = BindingContext as AdminUsersViewModel;
        if (vm != null)
        {
            await vm.LoadUsersCommand.ExecuteAsync(null);
        }
    }
}
