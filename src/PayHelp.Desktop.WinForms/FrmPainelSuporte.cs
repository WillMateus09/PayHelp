using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace PayHelp.Desktop.WinForms;

public partial class FrmPainelSuporte : Form
{
    private readonly ApiClient _api;
    private readonly SessionContext _session;

    public FrmPainelSuporte(ApiClient api, SessionContext session)
    {
        _api = api;
        _session = session;
        InitializeComponent();
        Theme.Apply(this);
        TicketStatus.Bind(cmbStatus);
        GridStyling.ApplyTicketsGrid(dgvTickets);


    dgvTickets.CellPainting += DgvTickets_CellPainting;
    dgvTickets.CellDoubleClick += DgvTickets_CellDoubleClick;


        try
        {
            filterRow.Padding = new Padding(0, 2, 0, 2);
            lblFiltro.Margin = new Padding(0, 6, 8, 6);
            cmbStatus.Margin = new Padding(0, 3, 12, 3);
            btnRecarregar.Margin = new Padding(0, 3, 0, 3);
            btnRecarregar.Padding = new Padding(12, 6, 12, 6);
            var targetH = cmbStatus.Height + 6;
            if (btnRecarregar.Height != targetH) btnRecarregar.Height = targetH;
            if (btnRecarregar.MinimumSize.Height != targetH)
                btnRecarregar.MinimumSize = new Size(btnRecarregar.MinimumSize.Width, targetH);
        }
        catch {  }

        this.Load += async (_, __) => await CarregarAsync();


    btnRecarregar.Tag = "secondary";
    btnAssumir.Tag = "primary";

    btnEmAtendimento.Visible = false;
    btnEncerrar.Visible = false;
        dgvTickets.SelectionChanged += (_, __) => UpdateButtonsState();
    }

    private ApiClient.TicketDto? Selecionado()
        => dgvTickets.CurrentRow?.DataBoundItem as ApiClient.TicketDto;

    private async Task CarregarAsync()
    {
        using var _ = LoadingOverlay.Show(this, "Carregando chamados...");
        string? status = cmbStatus.SelectedItem?.ToString();
        if (string.IsNullOrWhiteSpace(status)) status = null;
        var list = await _api.ListarChamadosAsync(status);
        var data = list ?? new List<ApiClient.TicketDto>();
        dgvTickets.DataSource = data;
        DecorateTriagingColumn();
        ToggleTriagingColumnVisibility(status);
    }

    private void DecorateTriagingColumn()
    {
        if (!dgvTickets.Columns.Contains("Triaging")) return;
        var col = dgvTickets.Columns["Triaging"];
        col.HeaderText = "Triagem";
        foreach (DataGridViewRow row in dgvTickets.Rows)
        {
            var cell = row.Cells["Triaging"];
            if (cell?.Value is bool b && b)
            {
                cell.Style.BackColor = Color.LightSkyBlue;
                cell.Style.ForeColor = Color.White;
                cell.Style.SelectionBackColor = Color.DeepSkyBlue;
                cell.Value = "IA em triagem";
            }
            else if (cell != null)
            {
                cell.Style.BackColor = Color.White;
                cell.Style.ForeColor = Color.Gray;
                cell.Value = "-";
            }
        }
    }

    private void ToggleTriagingColumnVisibility(string? statusFilter)
    {
        if (!dgvTickets.Columns.Contains("Triaging")) return;
        var hide = string.Equals(statusFilter, TicketStatus.Encerrado, StringComparison.OrdinalIgnoreCase);
        dgvTickets.Columns["Triaging"].Visible = !hide;
    }

    private async void cmbStatus_SelectedIndexChanged(object sender, EventArgs e)
    {
        await CarregarAsync();
    }

    private async void btnRecarregar_Click(object sender, EventArgs e)
    {
        await CarregarAsync();
    }

    private async void btnAssumir_Click(object sender, EventArgs e)
    {
        if (_session.CurrentUser is null)
        {
            MessageBox.Show("É preciso estar logado.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var sel = Selecionado(); if (sel is null) { UpdateButtonsState(); return; }
        using var _ = LoadingOverlay.Show(this, "Assumindo chamado...");
        var (ok, error) = await _api.AssumirChamadoAsync(sel.Id, _session.CurrentUser.UserId);
        if (!ok)
        {
            MessageBox.Show(string.IsNullOrWhiteSpace(error) ? "Falha ao assumir chamado." : error!,
                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        await CarregarAsync();
        UpdateButtonsState();


        try
        {
            var chat = new FrmChatChamado(_api, _session);
            chat.SetTicket(sel.Id);
            chat.Show();
        }
        catch { }
    }

    private async void btnEmAtendimento_Click(object sender, EventArgs e)
    {
        if (_session.CurrentUser is null) { UpdateButtonsState(); return; }
        var sel = Selecionado(); if (sel is null) { UpdateButtonsState(); return; }
        using var _ = LoadingOverlay.Show(this, "Atualizando status...");
        await _api.MudarStatusAsync(sel.Id, TicketStatus.EmAtendimento, _session.CurrentUser.UserId);
        await CarregarAsync();
        UpdateButtonsState();
    }

    private async void btnEncerrar_Click(object sender, EventArgs e)
    {
        var sel = Selecionado(); if (sel is null) { UpdateButtonsState(); return; }
        var confirm = MessageBox.Show("Encerrar o chamado selecionado?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;
        using var _ = LoadingOverlay.Show(this, "Encerrando chamado...");
        await _api.EncerrarChamadoAsync(sel.Id);

        var registrar = MessageBox.Show("Deseja registrar a resolução final na FAQ?", "Resolução Final", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (registrar == DialogResult.Yes)
        {
            using var modal = new PromptDialog("Informe a resolução final:");
            if (modal.ShowDialog(this) == DialogResult.OK)
            {
                var texto = modal.ResultText?.Trim();
                if (!string.IsNullOrWhiteSpace(texto))
                {
                    try { await _api.RegistrarResolucaoFinalAsync(sel.Id, texto); }
                    catch {  }
                }
            }
        }
        await CarregarAsync();
        UpdateButtonsState();
    }

    private void UpdateButtonsState()
    {
        var sel = Selecionado();
        var hasSel = sel != null;
        var canAct = _session.IsAuthenticated && _session.IsSupport;
        var status = sel?.Status ?? string.Empty;
        var isEncerrado = string.Equals(status, TicketStatus.Encerrado, StringComparison.OrdinalIgnoreCase);
        btnAssumir.Enabled = hasSel && canAct && !isEncerrado;
    }

    private void DgvTickets_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        var sel = Selecionado();
        if (sel is null) return;
        try
        {
            var chat = new FrmChatChamado(_api, _session);
            chat.SetTicket(sel.Id);
            chat.Show();
        }
        catch { }
    }




    private void DgvTickets_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0)
            return;

        var grid = (DataGridView)sender!;
        var col = grid.Columns[e.ColumnIndex];
        if (!string.Equals(col.HeaderText, "Status", StringComparison.OrdinalIgnoreCase))
            return;

        e.Handled = true;
        e.PaintBackground(e.ClipBounds, true);

        string? text = e.FormattedValue?.ToString();
        if (string.IsNullOrEmpty(text))
            return;


        Color backColor;
        Color textColor = Color.White;

        switch (text.Trim().ToLowerInvariant())
        {
            case "encerrado":
                backColor = Color.FromArgb(255, 210, 210);
                textColor = Color.DarkRed;
                break;
            case "aberto":
                backColor = Color.FromArgb(200, 220, 255);
                textColor = Color.DarkBlue;
                break;
            case "ematendimento":
            case "em atendimento":
                backColor = Color.FromArgb(190, 240, 230);
                textColor = Color.DarkCyan;
                break;
            default:
                backColor = Color.LightGray;
                textColor = Color.Black;
                break;
        }


        var textSize = TextRenderer.MeasureText(text, e.CellStyle.Font);
        int padH = 6;
        int padV = 2;
        int badgeWidth = textSize.Width + padH * 2;
        int badgeHeight = textSize.Height + padV * 2;

        int x = e.CellBounds.X + (e.CellBounds.Width - badgeWidth) / 2;
        int y = e.CellBounds.Y + (e.CellBounds.Height - badgeHeight) / 2;

        var badgeRect = new Rectangle(x, y, badgeWidth, badgeHeight);

        using (var path = RoundedRect(badgeRect, 10))
        using (var b = new SolidBrush(backColor))
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.FillPath(b, path);
        }

        TextRenderer.DrawText(
            e.Graphics,
            text,
            e.CellStyle.Font,
            badgeRect,
            textColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis
        );
    }

    private GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        int d = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
