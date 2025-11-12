using Microsoft.Maui.Controls;
using PayHelp.Mobile.Maui.Helpers;
using PayHelp.Mobile.Maui.ViewModels;

namespace PayHelp.Mobile.Maui.Views;

public partial class CadastroPage : ContentPage
{
    public CadastroPage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetService<CadastroViewModel>() ?? throw new InvalidOperationException("CadastroViewModel not resolved");
    }
}
