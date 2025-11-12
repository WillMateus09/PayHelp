using Microsoft.Maui.Controls;
using PayHelp.Mobile.Maui.Helpers;
using PayHelp.Mobile.Maui.ViewModels;

namespace PayHelp.Mobile.Maui.Views;

public partial class AbrirChamadoPage : ContentPage
{
    public AbrirChamadoPage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetService<AbrirChamadoViewModel>() ?? throw new InvalidOperationException("AbrirChamadoViewModel not resolved");
    }
}
