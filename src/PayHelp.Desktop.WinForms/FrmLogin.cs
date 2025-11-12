using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PayHelp.Desktop.WinForms;

public partial class FrmLogin : Form
{
    private readonly ApiClient _api;
    private readonly SessionContext _session;
    private readonly IServiceProvider _provider;

    public FrmLogin(ApiClient api, SessionContext session, IServiceProvider provider)
    {
        _api = api;
        _session = session;
        _provider = provider;
        InitializeComponent();


        Theme.Apply(this);
        this.AcceptButton = btnLogin;




        ApplyLayout();
    }

    private void ApplyLayout()
    {
        this.AutoScaleMode = AutoScaleMode.Font;
        this.MinimumSize = new Size(460, 220);
        this.ClientSize = new Size(460, 220);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Text = "PayHelp - Login";
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;


        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            Padding = new Padding(16),
        };

        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));


        var rowMargin = new Padding(0, 6, 0, 6);


        lblEmail.TextAlign = ContentAlignment.MiddleRight;
        lblSenha.TextAlign = ContentAlignment.MiddleRight;
        lblEmail.Margin = rowMargin;
        lblSenha.Margin = rowMargin;


        txtEmail.Dock = DockStyle.Fill;
        txtSenha.Dock = DockStyle.Fill;
        txtEmail.Margin = rowMargin;
        txtSenha.Margin = rowMargin;

        table.Controls.Add(lblEmail, 0, 0);
        table.Controls.Add(txtEmail, 1, 0);
        table.Controls.Add(lblSenha, 0, 1);
        table.Controls.Add(txtSenha, 1, 1);


        var bottom = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
        };
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        lnkCadastro.Anchor = AnchorStyles.Left;
        lnkCadastro.Margin = new Padding(0, 8, 0, 0);

        btnLogin.AutoSize = true;
        btnLogin.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        btnLogin.MinimumSize = new Size(120, 36);
        btnLogin.Margin = new Padding(0);
        btnLogin.Anchor = AnchorStyles.Right;

        bottom.Controls.Add(lnkCadastro, 0, 0);
        bottom.Controls.Add(new Panel { Dock = DockStyle.Fill }, 1, 0);
        bottom.Controls.Add(btnLogin, 2, 0);

        table.SetColumnSpan(bottom, 2);
        table.Controls.Add(bottom, 0, 2);


        this.Controls.Clear();
        this.Controls.Add(table);
    }

    private async void btnLogin_Click(object sender, EventArgs e)
    {
        try
        {
            ToggleUi(false);

            var email = txtEmail.Text.Trim().ToLowerInvariant();
            var senha = txtSenha.Text;
            AppLog.Write($"[LOGIN] Tentando autenticar '{email}'...");
            var auth = await _api.LoginAsync(email, senha);
            if (auth is null)
            {
                AppLog.Write("[LOGIN] Credenciais inválidas (401). Mantendo formulário aberto.");
                MessageBox.Show("Credenciais inválidas.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            AppLog.Write($"[LOGIN] Sucesso. UserId={auth.UserId}, Role={auth.Role}");
            _session.CurrentUser = auth;
            _session.AccessToken = auth.Token;
            MessageBox.Show($"Bem-vindo, {auth.Nome} ({auth.Role}).", "Login", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            AppLog.Write($"[LOGIN] Erro: {ex.GetType().Name} - {ex.Message}");
            MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            ToggleUi(true);
        }
    }

    private void ToggleUi(bool enabled)
    {
        txtEmail.Enabled = enabled;
        txtSenha.Enabled = enabled;
        btnLogin.Enabled = enabled;
    }

    private void lnkCadastro_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        var cadastro = (FrmCadastro)_provider.GetService(typeof(FrmCadastro))!;
        this.Hide();
        var result = cadastro.ShowDialog(this);
        this.Show();
        if (result == DialogResult.OK || _session.CurrentUser != null)
        {

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
