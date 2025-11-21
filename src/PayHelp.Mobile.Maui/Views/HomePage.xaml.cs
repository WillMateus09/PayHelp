using Microsoft.Maui.Controls;
using PayHelp.Mobile.Maui.ViewModels;

namespace PayHelp.Mobile.Maui.Views;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
        BindingContext = new HomeViewModel();
        Loaded += async (_, _) =>
        {
            if (BindingContext is HomeViewModel vm)
            {
                await vm.LoadAsync();
            }
        };
    }

    private async void OnNavigateClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is string route && !string.IsNullOrWhiteSpace(route))
        {
            await Shell.Current.GoToAsync($"//{route}");
        }
        else if (sender is Frame frame && frame.GestureRecognizers.FirstOrDefault() is TapGestureRecognizer tap && tap.CommandParameter is string frRoute && !string.IsNullOrWhiteSpace(frRoute))
        {
            // Para rotas registradas (como feedback-lista), usar navegação relativa
            if (frRoute == "feedback-lista")
            {
                await Shell.Current.GoToAsync(frRoute);
            }
            else
            {
                await Shell.Current.GoToAsync($"//{frRoute}");
            }
        }
    }
}
