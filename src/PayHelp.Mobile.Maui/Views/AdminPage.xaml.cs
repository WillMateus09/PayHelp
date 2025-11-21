namespace PayHelp.Mobile.Maui.Views;

public partial class AdminPage : ContentPage
{
    public AdminPage()
    {
        InitializeComponent();
    }

    private async void OnUsuariosClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AdminUsersPage());
    }

    private async void OnConfiguracoesClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AdminSettingsPage());
    }
}
