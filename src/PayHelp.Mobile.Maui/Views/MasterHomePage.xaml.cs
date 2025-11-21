using PayHelp.Mobile.Maui.ViewModels;

namespace PayHelp.Mobile.Maui.Views;

public partial class MasterHomePage : ContentPage
{
    public MasterHomePage()
    {
        InitializeComponent();
        BindingContext = new HomeViewModel();
    }

    private async void OnNavigateClicked(object sender, EventArgs e)
    {
        if (sender is VisualElement ve && ve.Parent is Frame frame)
        {
            if (frame.GestureRecognizers.FirstOrDefault() is TapGestureRecognizer tap && tap.CommandParameter is string route)
            {
                await Shell.Current.GoToAsync($"//{route}");
            }
        }
    }

    private async void OnAdminUsersClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AdminUsersPage());
    }

    private async void OnAdminSettingsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AdminSettingsPage());
    }
}
