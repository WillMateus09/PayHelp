using Microsoft.Maui.Controls;

namespace PayHelp.Mobile.Maui;

public partial class UserShell : Shell
{
    public UserShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("abrir-chamado", typeof(Views.AbrirChamadoPage));
        Routing.RegisterRoute("chamado-detalhe", typeof(Views.ChamadoDetalhePage));
        Routing.RegisterRoute("marcar-resolvido", typeof(Views.MarcarResolvidoPage));
    }
}
