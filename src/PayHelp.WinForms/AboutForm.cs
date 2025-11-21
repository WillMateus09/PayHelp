using System.Reflection;
using PayHelp.Client;

namespace PayHelp.WinForms;

public class AboutForm : Form
{
    private readonly ApiService _api;
    private readonly TextBox _txt = new() { Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical };

    public AboutForm(ApiService api)
    {
        _api = api;
        Text = "Sobre";
        Width = 600;
        Height = 400;
        Controls.Add(_txt);
        Load += async (_, __) => await LoadInfoAsync();
    }

    private async Task LoadInfoAsync()
    {
        var asm = Assembly.GetExecutingAssembly();
        var name = asm.GetName();
        var client = $"App: {name.Name}\r\nVersão: {name.Version}\r\nLocal: {asm.Location}";
        string server;
        try
        {
            server = await _api.GetServerVersionRawAsync() ?? "(indisponível)";
        }
        catch { server = "(erro ao consultar)"; }
        _txt.Text = client + "\r\n\r\nServidor __version:\r\n" + server;
    }
}
