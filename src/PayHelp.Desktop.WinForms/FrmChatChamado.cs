using System.Windows.Forms;
namespace PayHelp.Desktop.WinForms;

public partial class FrmChatChamado : Form
{
    private readonly ApiClient _api;
    private readonly SessionContext _session;
    private Guid? _initialTicketId;
    private System.Windows.Forms.Timer? _timer;
    private readonly Button _btnChamarAtendente = new Button();
    private readonly Button _btnEncerrar = new Button();
    private bool _triagemFeitaParaTicketAtual;

    public FrmChatChamado(ApiClient api, SessionContext session)
    {
        _api = api;
        _session = session;
        InitializeComponent();
        Theme.Apply(this);
        this.Load += async (_, __) => await CarregarTicketsAsync(selectTicketId: _initialTicketId);
        ApplyLayout();
    IniciarAutoRefresh();


        this.txtMensagem.KeyDown += (s, e) =>
        {
            if (e.Control && e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                btnEnviar.PerformClick();
            }
        };
    }

    public void SetTicket(Guid ticketId)
    {
        _initialTicketId = ticketId;
    }

    private async Task CarregarTicketsAsync(Guid? selectTicketId = null)
    {
        if (_session.CurrentUser is null) return;
        using var _ = LoadingOverlay.Show(this, "Carregando tickets...");
        List<ApiClient.TicketDto> items;
        if (_session.IsSupport)
            items = await (_api.ListarChamadosAsync(null) ?? Task.FromResult<List<ApiClient.TicketDto>?>(null)) ?? new List<ApiClient.TicketDto>();
        else
            items = await (_api.ListarMeusChamadosAsync(_session.CurrentUser.UserId) ?? Task.FromResult<List<ApiClient.TicketDto>?>(null)) ?? new List<ApiClient.TicketDto>();
        cmbTickets.DisplayMember = nameof(ApiClient.TicketDto.Titulo);
        cmbTickets.ValueMember = nameof(ApiClient.TicketDto.Id);
        cmbTickets.DataSource = items;
        if (selectTicketId.HasValue)
        {
            var idx = items.FindIndex(t => t.Id == selectTicketId.Value);
            if (idx >= 0) cmbTickets.SelectedIndex = idx;
        }
        await CarregarMensagensAsync();
        _triagemFeitaParaTicketAtual = false;
        _btnChamarAtendente.Enabled = false;
        lblSugestao.Text = "Sugestão: (vazia)";
        UpdateHeaderBadge();
        UpdateActionButtons();
    }

    private ApiClient.TicketDto? TicketAtual() => cmbTickets.SelectedItem as ApiClient.TicketDto;

    private async Task CarregarMensagensAsync()
    {
        using var _ = LoadingOverlay.Show(this, "Carregando mensagens...");
        lstMensagens.Items.Clear();
        var ticket = TicketAtual();
        if (ticket is null) return;
        var det = await _api.ObterChamadoAsync(ticket.Id);
        if (det is null) return;
        foreach (var m in det.Mensagens.OrderBy(m => m.EnviadoEmUtc))
        {
            var who = m.Automatica ? "[BOT]" : (m.RemetenteUserId == _session.CurrentUser?.UserId ? "Você" : "Outro");
            var localTime = DateTime.SpecifyKind(m.EnviadoEmUtc, DateTimeKind.Utc).ToLocalTime();
            lstMensagens.Items.Add($"{localTime:dd/MM HH:mm} {who}: {m.Conteudo}");
        }
        if (lstMensagens.Items.Count > 0)
            lstMensagens.TopIndex = lstMensagens.Items.Count - 1;
        UpdateHeaderBadge();
        UpdateActionButtons();
    }

    private void UpdateHeaderBadge()
    {
        var ticket = TicketAtual();
        var badge = this.Controls.Find("lblTriaging", true).FirstOrDefault() as Label;
        if (badge == null) return;
        var triaging = ticket?.Triaging ?? false;
        badge.Visible = triaging;
    }

    private async void btnRecarregar_Click(object? sender, EventArgs e)
    {
        await CarregarTicketsAsync(TicketAtual()?.Id);
    }

    private async void cmbTickets_SelectedIndexChanged(object? sender, EventArgs e)
    {
        await CarregarMensagensAsync();
        _triagemFeitaParaTicketAtual = false;
        _btnChamarAtendente.Enabled = false;
        lblSugestao.Text = "Sugestão: (vazia)";
        UpdateActionButtons();
    }

    private async void btnEnviar_Click(object? sender, EventArgs e)
    {
        if (_session.CurrentUser is null) return;
        var ticket = TicketAtual();
        if (ticket is null) return;
        var text = txtMensagem.Text.Trim();
        if (string.IsNullOrWhiteSpace(text)) return;

        var det = await _api.ObterChamadoAsync(ticket.Id);
        if (det?.EncerradoEmUtc != null)
        {
            MessageBox.Show("Chamado está encerrado. Não é possível enviar mensagens.", "Chat", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var _ = LoadingOverlay.Show(this, "Enviando...");
        try
        {
            var ok = await _api.EnviarMensagemAsync(ticket.Id, _session.CurrentUser.UserId, text, automatica: false);
            if (ok) { txtMensagem.Clear(); await CarregarMensagensAsync(); }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Falha ao enviar mensagem", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnTriagem_Click(object? sender, EventArgs e)
    {
        var input = txtMensagem.Text.Trim();
        if (string.IsNullOrWhiteSpace(input))
        {

            var ticket = TicketAtual();
            if (ticket != null)
            {
                try
                {
                    var det = await _api.ObterChamadoAsync(ticket.Id);
                    input = (det?.Mensagens ?? new List<ApiClient.TicketMessageDto>())
                        .Where(m => !m.Automatica && m.RemetenteUserId == _session.CurrentUser?.UserId)
                        .OrderByDescending(m => m.EnviadoEmUtc)
                        .Select(m => m.Conteudo)
                        .FirstOrDefault()
                        ?? (det?.Titulo ?? string.Empty);
                }
                catch { input = string.Empty; }
            }
            if (string.IsNullOrWhiteSpace(input)) return;
        }
        using var _ = LoadingOverlay.Show(this, "Analisando...");
        var suggestion = await _api.SugerirAsync(input);
        if (!string.IsNullOrWhiteSpace(suggestion))
        {
            lblSugestao.Text = $"Sugestão: {suggestion}";

            var ticket = TicketAtual();
            if (ticket != null && _session.CurrentUser != null)
            {
                try { await _api.EnviarMensagemAsync(ticket.Id, _session.CurrentUser.UserId, suggestion, automatica: true); await CarregarMensagensAsync(); }
                catch (Exception ex) { MessageBox.Show(this, ex.Message, "Falha ao registrar sugestão", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            }
        }

        _triagemFeitaParaTicketAtual = true;
        _btnChamarAtendente.Enabled = true;
    }

    private void ApplyLayout()
    {

        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.MinimumSize = new System.Drawing.Size(720, 480);
        this.StartPosition = FormStartPosition.CenterParent;

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(12) };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var header = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 3, RowCount = 1 };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 12));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        cmbTickets.Dock = DockStyle.Fill;
    btnRecarregar.Tag = "secondary"; btnRecarregar.AutoSize = true; btnRecarregar.Height = 32;
        header.Controls.Add(cmbTickets, 0, 0);
        header.Controls.Add(new Panel { Width = 1 }, 1, 0);
        header.Controls.Add(btnRecarregar, 2, 0);

        lstMensagens.Dock = DockStyle.Fill;

        var input = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 3, RowCount = 1 };
        input.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        input.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 12));
        input.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        txtMensagem.Dock = DockStyle.Fill;
    btnEnviar.Tag = "primary"; btnEnviar.AutoSize = true; btnEnviar.Height = 32;
        input.Controls.Add(txtMensagem, 0, 0);
        input.Controls.Add(new Panel { Width = 1 }, 1, 0);
        input.Controls.Add(btnEnviar, 2, 0);

    var triagem = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 4, RowCount = 1 };
        triagem.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        triagem.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        triagem.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        triagem.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

    btnTriagem.Tag = "primary"; btnTriagem.Width = 120; btnTriagem.Height = 30;

    _btnChamarAtendente.Tag = "primary"; _btnChamarAtendente.AutoSize = true; _btnChamarAtendente.Width = 140; _btnChamarAtendente.Height = 30; _btnChamarAtendente.Text = "Chamar atendente"; _btnChamarAtendente.Click += btnChamarAtendente_Click;

    _btnEncerrar.Tag = "danger"; _btnEncerrar.AutoSize = true; _btnEncerrar.Width = 110; _btnEncerrar.Height = 30; _btnEncerrar.Text = "Encerrar"; _btnEncerrar.Click += btnEncerrar_Click;
        lblSugestao.AutoSize = true;
    triagem.Controls.Add(btnTriagem, 0, 0);
    triagem.Controls.Add(_btnChamarAtendente, 1, 0);
    triagem.Controls.Add(lblSugestao, 2, 0);
    triagem.Controls.Add(_btnEncerrar, 3, 0);

        root.Controls.Add(header, 0, 0);
        root.Controls.Add(lstMensagens, 0, 1);
        root.Controls.Add(input, 0, 2);
        root.Controls.Add(triagem, 0, 3);
    this.Controls.Clear();
    this.Controls.Add(root);

    Theme.Apply(this);
        _btnChamarAtendente.Enabled = false;
        UpdateActionButtons();
    }

    private void IniciarAutoRefresh()
    {
        _timer = new System.Windows.Forms.Timer { Interval = 5000 };
        _timer.Tick += async (_, __) =>
        {
            try { await CarregarMensagensAsync(); } catch {  }
        };
        _timer.Start();
        this.FormClosed += (_, __) => { try { _timer?.Stop(); _timer?.Dispose(); } catch { } };
    }

    private async void btnChamarAtendente_Click(object? sender, EventArgs e)
    {
        var ticket = TicketAtual();
        if (ticket is null) return;
        using var _ = LoadingOverlay.Show(this, "Chamando atendente...");
        var (ok, error) = await _api.ChamarAtendenteAsync(ticket.Id);
        if (!ok)
        {
            MessageBox.Show(this, string.IsNullOrWhiteSpace(error) ? "Não foi possível chamar um atendente." : error!, "Atendimento", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        await CarregarMensagensAsync();
        MessageBox.Show(this, "Um atendente foi acionado para este chamado.", "Atendimento", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void UpdateActionButtons()
    {
        var ticket = TicketAtual();
        var encerrado = string.Equals(ticket?.Status, TicketStatus.Encerrado, StringComparison.OrdinalIgnoreCase);
        if (_session.IsSupport)
        {
            btnTriagem.Visible = false;
            _btnChamarAtendente.Visible = false;
            _btnEncerrar.Visible = true;
            _btnEncerrar.Enabled = ticket != null && !encerrado;
        }
        else
        {
            btnTriagem.Visible = true;
            _btnChamarAtendente.Visible = true;
            _btnChamarAtendente.Enabled = _triagemFeitaParaTicketAtual;
            _btnEncerrar.Visible = false;
        }
    }

    private async void btnEncerrar_Click(object? sender, EventArgs e)
    {
        if (!_session.IsSupport) return;
        var ticket = TicketAtual(); if (ticket is null) return;
        if (string.Equals(ticket.Status, TicketStatus.Encerrado, StringComparison.OrdinalIgnoreCase)) return;
        var ok = false; string? error = null;
        using (var _ = LoadingOverlay.Show(this, "Encerrando chamado..."))
        {
            try { await _api.EncerrarChamadoAsync(ticket.Id); ok = true; }
            catch (Exception ex) { error = ex.Message; }
        }
        if (!ok)
        {
            MessageBox.Show(this, string.IsNullOrWhiteSpace(error) ? "Não foi possível encerrar o chamado." : error!, "Encerrar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        try
        {
            using var modal = new PromptDialog("Informe a resolução final:");
            if (modal.ShowDialog(this) == DialogResult.OK)
            {
                var texto = modal.ResultText?.Trim();
                if (!string.IsNullOrWhiteSpace(texto))
                {
                    try { await _api.RegistrarResolucaoFinalAsync(ticket.Id, texto!); } catch { }
                }
            }
        }
        catch { }
        await CarregarTicketsAsync(selectTicketId: ticket.Id);
        await CarregarMensagensAsync();
        UpdateActionButtons();
    }
}
