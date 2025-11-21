using PayHelp.Client;

namespace PayHelp.WinForms;

public class ChatForm : Form
{
    private readonly ChatClientService _chat;
    private readonly ITokenStore _tokenStore;
    private readonly ListBox _list = new() { Dock = DockStyle.Fill };
    private readonly Label _status = new() { Dock = DockStyle.Top, Height = 24 };

    public ChatForm(ChatClientService chat, ITokenStore tokenStore)
    {
        _chat = chat;
        _tokenStore = tokenStore;
        Text = "PayHelp - Chat";
        Width = 600;
        Height = 400;
        Controls.Add(_list);
        Controls.Add(_status);

        _chat.OnConnected += () => this.BeginInvoke(() => _status.Text = "Conectado");
        _chat.OnDisconnected += () => this.BeginInvoke(() => _status.Text = "Desconectado");
        _chat.OnMessageReceived += (user, msg) => this.BeginInvoke(() => _list.Items.Add($"{user}: {msg}"));

        Load += async (_, __) =>
        {
            try
            {
                await _chat.ConnectAsync();
            }
            catch (ApiUnauthorizedException)
            {
                await _tokenStore.ClearAsync();
                MessageBox.Show("Sessão expirada. Faça login novamente.");
                Close();
            }
        };
        FormClosed += async (_, __) => await _chat.DisconnectAsync();
    }
}
