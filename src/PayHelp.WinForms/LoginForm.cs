using Microsoft.Extensions.DependencyInjection;
using PayHelp.Client;

namespace PayHelp.WinForms;

public class LoginForm : Form
{
    private readonly ApiService _api;
    private readonly ITokenStore _tokenStore;
    private TextBox _email = null!;
    private TextBox _senha = null!;
    private Label _status = null!;

    public LoginForm(ApiService api, ITokenStore tokenStore)
    {
        _api = api;
        _tokenStore = tokenStore;
        Text = "Login - PayHelp";
        Width = 400;
        Height = 220;
        StartPosition = FormStartPosition.CenterScreen;
        BuildUi();
    }

    private void BuildUi()
    {
        var lblEmail = new Label { Text = "E-mail", Left = 20, Top = 20, Width = 80 };
        _email = new TextBox { Left = 110, Top = 18, Width = 240 };
        var lblSenha = new Label { Text = "Senha", Left = 20, Top = 60, Width = 80 };
        _senha = new TextBox { Left = 110, Top = 58, Width = 240, UseSystemPasswordChar = true };
        var btn = new Button { Text = "Entrar", Left = 110, Top = 100, Width = 120 };
        _status = new Label { Left = 20, Top = 140, Width = 330, ForeColor = Color.Red };
        btn.Click += async (_, __) => await DoLoginAsync();
        Controls.AddRange(new Control[] { lblEmail, _email, lblSenha, _senha, btn, _status });
    }

    private async Task DoLoginAsync()
    {
        try
        {
            _status.Text = string.Empty;
            var token = await _api.LoginAsync(_email.Text, _senha.Text);
            await _tokenStore.SetAsync(token);

            var main = Program.Services.GetRequiredService<MainForm>();
            main.FormClosed += (_, __) => this.Close();
            main.Show();
            Hide();
        }
        catch (ApiUnauthorizedException)
        {
            await _tokenStore.ClearAsync();
            _status.Text = "Credenciais inválidas.";
        }
        catch (Exception ex)
        {
            _status.Text = ex.Message;
        }
    }
}
