using Microsoft.Maui.Controls;
using PayHelp.Mobile.Maui.Helpers;
using PayHelp.Mobile.Maui.ViewModels;

namespace PayHelp.Mobile.Maui.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetService<LoginViewModel>() ?? new LoginViewModel(ServiceHelper.GetService<Services.AuthService>()!);
    }
}
