using Microsoft.Maui.Controls;

namespace PayHelp.Mobile.Maui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("abrir-chamado", typeof(Views.AbrirChamadoPage));
        Routing.RegisterRoute("chamado-detalhe", typeof(Views.ChamadoDetalhePage));
    }
}
