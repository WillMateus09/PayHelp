using System.Threading.Tasks;
using System.Windows.Forms;

namespace PayHelp.Desktop.WinForms;

public partial class FrmCadastro : Form
{
    private readonly ApiClient _api;
    private readonly SessionContext _session;
    private readonly IServiceProvider _provider;

    public FrmCadastro(ApiClient api, SessionContext session, IServiceProvider provider)
    {
        _api = api;
        _session = session;
        _provider = provider;
        InitializeComponent();

        Theme.Apply(this);
        this.AcceptButton = btnRegistrarSimples;
    }

    private async void btnRegistrarSimples_Click(object sender, System.EventArgs e)
    {
        await Registrar(false);
    }

    private async void btnRegistrarSuporte_Click(object sender, System.EventArgs e)
    {
        await Registrar(true);
    }

    private async Task Registrar(bool suporte)
    {
        try
        {
            ToggleUi(false);
            var numero = txtNumero.Text.Trim();
            var nome = txtNome.Text.Trim();

            var email = txtEmail.Text.Trim().ToLowerInvariant();
            var senha = txtSenha.Text;
            var confirmar = txtConfirmarSenha.Text;
            var palavra = txtPalavra.Text;


            if (string.IsNullOrWhiteSpace(numero) || string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
            {
                MessageBox.Show("Preencha todos os campos obrigatórios.", "Cadastro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!email.Contains('@') || !email.Contains('.'))
            {
                MessageBox.Show("E-mail inválido.", "Cadastro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (senha.Length < 6)
            {
                MessageBox.Show("Senha deve ter pelo menos 6 caracteres.", "Cadastro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!string.Equals(senha, confirmar))
            {
                MessageBox.Show("As senhas não conferem.", "Cadastro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (suporte && string.IsNullOrWhiteSpace(palavra))
            {
                MessageBox.Show("Informe a palavra de verificação do suporte.", "Cadastro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var _ = LoadingOverlay.Show(this, "Registrando...");
            ApiClient.AuthResponse auth = suporte
                ? await _api.RegistrarSuporteAsync(numero, nome, email, senha, palavra)
                : await _api.RegistrarSimplesAsync(numero, nome, email, senha);

            _session.CurrentUser = auth;
            _session.AccessToken = auth.Token;
            MessageBox.Show("Cadastro realizado com sucesso!", "Cadastro", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (System.Exception ex)
        {
            MessageBox.Show(ex.Message, "Erro no cadastro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            ToggleUi(true);
        }
    }

    private void ToggleUi(bool enabled)
    {
        foreach (Control c in Controls) c.Enabled = enabled;
    }

    private void lblPalavra_Click(object sender, System.EventArgs e)
    {

    }
}
