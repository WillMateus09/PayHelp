using System.Windows.Forms;
namespace PayHelp.Desktop.WinForms;

public partial class FrmMensagensAutomaticas : Form
{
    private readonly ApiClient _api;
    public FrmMensagensAutomaticas(ApiClient api)
    {
        _api = api;
        InitializeComponent();
        Theme.Apply(this);
        this.Load += async (_, __) => await CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        using var _ = LoadingOverlay.Show(this, "Carregando mensagens...");
    var list = await _api.ListarMensagensAutomaticasAsync();
    var data = list ?? new List<ApiClient.CannedMessageDto>();
    dgv.AutoGenerateColumns = true;
    dgv.DataSource = data;

    if (dgv.Columns.Contains("Id")) dgv.Columns["Id"].Visible = false;
    if (dgv.Columns.Contains("Titulo")) dgv.Columns["Titulo"].HeaderText = "Título";
    if (dgv.Columns.Contains("Conteudo")) dgv.Columns["Conteudo"].HeaderText = "Conteúdo";
    if (dgv.Columns.Contains("GatilhoPalavraChave")) dgv.Columns["GatilhoPalavraChave"].HeaderText = "Gatilhos";
    }

    private async void btnRecarregar_Click(object sender, EventArgs e)
    {
        await CarregarAsync();
    }

    private async void btnCriar_Click(object sender, EventArgs e)
    {
    var titulo = txtTitulo.Text.Trim();
    var conteudo = txtConteudo.Text.Trim();
    var gatilhos = txtGatilhos.Text.Trim();
        if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(conteudo)) return;
        using var _ = LoadingOverlay.Show(this, "Criando mensagem...");
    var created = await _api.CriarMensagemAutomaticaAsync(titulo, conteudo, string.IsNullOrWhiteSpace(gatilhos) ? null : gatilhos);
    if (created != null) { txtTitulo.Clear(); txtConteudo.Clear(); txtGatilhos.Clear(); txtTitulo.Focus(); }
        await CarregarAsync();
    }

    private async void btnRemover_Click(object sender, EventArgs e)
    {
        if (dgv.CurrentRow?.DataBoundItem is ApiClient.CannedMessageDto msg)
        {
            using var _ = LoadingOverlay.Show(this, "Removendo...");
            await _api.RemoverMensagemAutomaticaAsync(msg.Id);
            await CarregarAsync();
        }
    }
}
