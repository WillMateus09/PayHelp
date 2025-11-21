using Microsoft.Extensions.DependencyInjection;
using PayHelp.Client;
using PayHelp.Client.Dtos;

namespace PayHelp.WinForms;

public class MainForm : Form
{
    private readonly ApiService _api;
    private readonly ITokenStore _tokenStore;
    private readonly IServiceProvider _sp;
    private readonly DataGridView _grid = new();
    private readonly Label _status = new() { ForeColor = Color.Red, AutoSize = true };

    public MainForm(ApiService api, ITokenStore tokenStore, IServiceProvider sp)
    {
        _api = api;
        _tokenStore = tokenStore;
        _sp = sp;
        Text = "PayHelp - Tickets";
        Width = 800;
        Height = 500;
        StartPosition = FormStartPosition.CenterScreen;
        BuildUi();
        Load += async (_, __) => await ReloadAsync();
    }

    private void BuildUi()
    {
        _grid.Dock = DockStyle.Top;
        _grid.Height = 380;
        _grid.ReadOnly = true;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.AutoGenerateColumns = true;

        var btnResolve = new Button { Text = "Resolver", Left = 20, Top = 390, Width = 100 };
        var btnFeedbacks = new Button { Text = "Feedbacks", Left = 140, Top = 390, Width = 100 };
        var btnChat = new Button { Text = "Chat", Left = 260, Top = 390, Width = 100 };
        var btnAbout = new Button { Text = "Sobre", Left = 380, Top = 390, Width = 100 };
        btnResolve.Click += async (_, __) => await ResolveSelectedAsync();
        btnFeedbacks.Click += (_, __) =>
        {
            if (_grid.CurrentRow?.DataBoundItem is TicketDto t)
            {
                var form = new FeedbacksForm(_api, _tokenStore, t.Id);
                form.Show();
            }
        };
        btnChat.Click += (_, __) =>
        {
            var chat = _sp.GetRequiredService<ChatForm>();
            chat.Show();
        };
        btnAbout.Click += (_, __) => _sp.GetRequiredService<AboutForm>().ShowDialog();

        Controls.AddRange(new Control[] { _grid, btnResolve, btnFeedbacks, btnChat, btnAbout, _status });
    }

    private async Task ReloadAsync()
    {
        try
        {
            _status.Text = string.Empty;
            var data = (await _api.GetTicketsAsync()).ToList();
            _grid.DataSource = data;
        }
        catch (ApiUnauthorizedException)
        {
            await _tokenStore.ClearAsync();
            MessageBox.Show("Sessão expirada. Faça login novamente.", "Acesso negado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            var login = _sp.GetRequiredService<LoginForm>();
            login.Show();
            Close();
        }
        catch (Exception ex)
        {
            _status.Text = ex.Message;
        }
    }

    private async Task ResolveSelectedAsync()
    {
        if (_grid.CurrentRow?.DataBoundItem is not TicketDto t)
            return;
        try
        {
            // Ajustar usuarioId conforme contexto real
            var usuarioId = Guid.NewGuid();
            await _api.ResolverPeloUsuarioAsync(t.Id, usuarioId, "Resolvido via WinForms", 5);
            await ReloadAsync();
        }
        catch (ApiUnauthorizedException)
        {
            await _tokenStore.ClearAsync();
            MessageBox.Show("Sessão expirada. Faça login novamente.");
            var login = _sp.GetRequiredService<LoginForm>();
            login.Show();
            Close();
        }
        catch (Exception ex)
        {
            _status.Text = ex.Message;
        }
    }
}
