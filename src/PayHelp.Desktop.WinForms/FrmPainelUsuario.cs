using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace PayHelp.Desktop.WinForms;

public partial class FrmPainelUsuario : Form
{
    private readonly ApiClient _api;
    private readonly SessionContext _session;
    private readonly IServiceProvider _provider;

    public FrmPainelUsuario(ApiClient api, SessionContext session, IServiceProvider provider)
    {
        _api = api;
        _session = session;
        _provider = provider;
        InitializeComponent();
        Theme.Apply(this);
        GridStyling.ApplyTicketsGrid(dgvTickets);


        try
        {

            btnRecarregar.Tag = "secondary";
            btnRecarregar.Margin = new Padding(0, 3, 0, 3);
            btnRecarregar.Padding = new Padding(12, 6, 12, 6);
            var targetTopH = TextRenderer.MeasureText("Ag", this.Font).Height + 10;
            if (btnRecarregar.Height != targetTopH) btnRecarregar.Height = targetTopH;
            if (btnRecarregar.MinimumSize.Height != targetTopH)
                btnRecarregar.MinimumSize = new Size(btnRecarregar.MinimumSize.Width, targetTopH);


            btnAbrirChat.Tag = "primary";
            btnNovo.Tag = "secondary";
            btnAbrirChat.Margin = new Padding(0, 0, 12, 0);
            btnNovo.Margin = new Padding(0);
            var lineH = TextRenderer.MeasureText("Ag", btnAbrirChat.Font).Height + 14;
            foreach (var b in new[] { btnAbrirChat, btnNovo })
            {
                if (b.Height != lineH) b.Height = lineH;
                if (b.MinimumSize.Height != lineH)
                    b.MinimumSize = new Size(Math.Max(110, b.MinimumSize.Width), lineH);

                b.Padding = new Padding(14, 6, 14, 6);
            }
        }
        catch {  }


        dgvTickets.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 237, 255);
        dgvTickets.DefaultCellStyle.SelectionForeColor = Color.Black;
        dgvTickets.EditMode = DataGridViewEditMode.EditProgrammatically;


        dgvTickets.CellPainting += DgvTickets_CellPainting;
        dgvTickets.CellFormatting += DgvTickets_CellFormatting;

    dgvTickets.DataError += (s, e) => { e.ThrowException = false; };

        this.Load += async (_, __) => await CarregarTicketsAsync();
    }

    private async Task CarregarTicketsAsync()
    {
        if (_session.CurrentUser is null) return;
        using var _ = LoadingOverlay.Show(this, "Carregando seus chamados...");
        var lista = await _api.ListarMeusChamadosAsync(_session.CurrentUser.UserId) ?? new List<ApiClient.TicketDto>();
        dgvTickets.AutoGenerateColumns = true;
        dgvTickets.DataSource = lista;


        EnsureStatusTextColumn();


        EnsureTriagingTextColumn();
        DecorateTriagingColumn();
        ToggleTriagingColumnVisibility();
    }

    private void ToggleTriagingColumnVisibility()
    {
        if (!dgvTickets.Columns.Contains("Triaging")) return;

        bool allClosed = true;
        var statusCol = FindStatusColumn();
        if (statusCol is null)
        {

            dgvTickets.Columns["Triaging"].Visible = true;
            return;
        }
        foreach (DataGridViewRow row in dgvTickets.Rows)
        {
            var status = Convert.ToString(row.Cells[statusCol.Index]?.Value) ?? string.Empty;
            if (!string.Equals(status, TicketStatus.Encerrado, StringComparison.OrdinalIgnoreCase))
            {
                allClosed = false; break;
            }
        }
        dgvTickets.Columns["Triaging"].Visible = !allClosed;
    }

    private void DecorateTriagingColumn()
    {
        if (!dgvTickets.Columns.Contains("Triaging")) return;
        var col = dgvTickets.Columns["Triaging"]; col.HeaderText = "Triagem";
        foreach (DataGridViewRow row in dgvTickets.Rows)
        {
            var cell = row.Cells["Triaging"];
            if (cell?.Value is bool b && b)
            {
                cell.Style.BackColor = Color.FromArgb(232, 244, 253);
                cell.Style.ForeColor = Color.FromArgb(26, 95, 180);
                cell.Style.SelectionBackColor = Color.FromArgb(177, 212, 243);
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

    private async void btnRecarregar_Click(object sender, EventArgs e)
    {
        await CarregarTicketsAsync();
    }

    private void dgvTickets_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        if (dgvTickets.CurrentRow?.DataBoundItem is ApiClient.TicketDto sel)
        {
            AbrirChat(sel);
        }
    }

    private async void btnAbrir_Click(object sender, EventArgs e)
    {
        if (_session.CurrentUser is null)
        {
            MessageBox.Show("Você precisa estar logado.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var titulo = txtTitulo.Text.Trim();
        var desc = txtDesc.Text.Trim();
        if (string.IsNullOrWhiteSpace(titulo))
        {
            MessageBox.Show("Informe um título.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var t = await _api.AbrirChamadoAsync(_session.CurrentUser.UserId, titulo, desc);
        if (t is null)
        {
            MessageBox.Show("Falha ao abrir chamado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        txtTitulo.Clear(); txtDesc.Clear();
        await CarregarTicketsAsync();
    }

    private void btnNovo_Click(object sender, EventArgs e)
    {

        if (_provider.GetService(typeof(FrmAbrirChamadoWizard)) is FrmAbrirChamadoWizard wiz)
        {
            wiz.ShowDialog();
        }
        else
        {
            MessageBox.Show("Wizard não disponível nesta build.");
        }
    }

    private void btnAbrirChat_Click(object sender, EventArgs e)
    {
        if (dgvTickets.CurrentRow?.DataBoundItem is ApiClient.TicketDto sel)
        {
            AbrirChat(sel);
        }
    }

    private async void AbrirChat(ApiClient.TicketDto ticket)
    {
        var chat = (FrmChatChamado)_provider.GetService(typeof(FrmChatChamado))!;
        chat.SetTicket(ticket.Id);
        chat.ShowDialog();
        
        // Recarrega os tickets após fechar o chat para atualizar o status
        await CarregarTicketsAsync();
    }



    private void EnsureStatusTextColumn()
    {
        var col = dgvTickets.Columns
            .Cast<DataGridViewColumn>()
            .FirstOrDefault(c =>
                string.Equals(c.DataPropertyName, "Status", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.HeaderText, "Status", StringComparison.OrdinalIgnoreCase));

        if (col is DataGridViewButtonColumn btn)
        {
            var idx = btn.Index;
            var replacement = new DataGridViewTextBoxColumn
            {
                HeaderText = btn.HeaderText,
                DataPropertyName = btn.DataPropertyName,
                Name = "Status",
                AutoSizeMode = btn.AutoSizeMode,
                FillWeight = btn.FillWeight,
                ReadOnly = true
            };
            dgvTickets.Columns.RemoveAt(idx);
            dgvTickets.Columns.Insert(idx, replacement);
        }
    }



    private void EnsureTriagingTextColumn()
    {
        var col = dgvTickets.Columns
            .Cast<DataGridViewColumn>()
            .FirstOrDefault(c =>
                string.Equals(c.DataPropertyName, "Triaging", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.Name, "Triaging", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.HeaderText, "Triaging", StringComparison.OrdinalIgnoreCase));

        if (col is null) return;


        if (col is DataGridViewTextBoxColumn txt && string.IsNullOrEmpty(txt.DataPropertyName))
            return;

        var idx = col.Index;
        var replacement = new DataGridViewTextBoxColumn
        {
            HeaderText = "Triagem",
            Name = "Triaging",
            ReadOnly = true,
            AutoSizeMode = col.AutoSizeMode,
            FillWeight = col.FillWeight,

            DataPropertyName = string.Empty
        };

        dgvTickets.Columns.RemoveAt(idx);
        dgvTickets.Columns.Insert(idx, replacement);
    }

    private void DgvTickets_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0) return;
        if (IsStatusColumn(e.ColumnIndex) && e.Value is string s)
        {

            var nice = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLowerInvariant());
            e.Value = nice;
            e.FormattingApplied = true;
        }
        else if (IsTriagingColumn(e.ColumnIndex))
        {

            var v = e.Value;
            if (v is bool b && b)
            {
                e.Value = "IA em triagem";
                e.FormattingApplied = true;
            }
            else if (v is null || (v is bool bb && !bb))
            {
                e.Value = "-";
                e.FormattingApplied = true;
            }
        }
    }

    private void DgvTickets_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || !IsStatusColumn(e.ColumnIndex)) return;


        e.Handled = true;


        e.PaintBackground(e.CellBounds, true);

        var text = Convert.ToString(e.FormattedValue) ?? string.Empty;


        var font = e.CellStyle.Font ?? dgvTickets.Font;
        var textSize = TextRenderer.MeasureText(text, font);


        var padH = 8;
        var padV = 4;
        var badgeWidth = Math.Min(textSize.Width + padH * 2, Math.Max(32, e.CellBounds.Width - 8));
        var badgeHeight = Math.Min(textSize.Height + padV * 2, Math.Max(18, e.CellBounds.Height - 8));

        var x = e.CellBounds.X + 8;
        var y = e.CellBounds.Y + (e.CellBounds.Height - badgeHeight) / 2;
        var badgeRect = new Rectangle(x, y, badgeWidth, badgeHeight);


        var (back, border, fore) = GetStatusColors(text);

        using var bg = new SolidBrush(back);
        using var pen = new Pen(border);
        using var path = CreateRoundRect(badgeRect, 7);

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.FillPath(bg, path);
        e.Graphics.DrawPath(pen, path);


        var tf = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
        TextRenderer.DrawText(e.Graphics, text, font, badgeRect, fore, tf);


        e.Paint(e.CellBounds, DataGridViewPaintParts.Border);
    }

    private static (Color back, Color border, Color fore) GetStatusColors(string status)
    {

        switch (status.Trim().ToLowerInvariant())
        {
            case "aberto":
                return (Color.FromArgb(227, 245, 231), Color.FromArgb(170, 225, 180), Color.FromArgb(20, 111, 37));
            case "triagem":
                return (Color.FromArgb(255, 249, 230), Color.FromArgb(255, 224, 102), Color.FromArgb(138, 106, 0));
            case "em andamento":
            case "andamento":
            case "ematendimento":
                return (Color.FromArgb(232, 244, 253), Color.FromArgb(177, 212, 243), Color.FromArgb(26, 95, 180));
            case "resolvido pelo usuário (ia)":
            case "resolvido":
            case "resolvidopelousuario":
                return (Color.FromArgb(220, 237, 200), Color.FromArgb(139, 195, 74), Color.FromArgb(51, 105, 30));
            case "fechado":
            case "encerrado":
                return (Color.FromArgb(238, 238, 238), Color.FromArgb(210, 210, 210), Color.FromArgb(90, 90, 90));
            default:
                return (Color.FromArgb(234, 234, 234), Color.FromArgb(210, 210, 210), Color.FromArgb(60, 60, 60));
        }
    }

    private static GraphicsPath CreateRoundRect(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        int d = radius * 2;
        var arc = new Rectangle(rect.Location, new Size(d, d));


        path.AddArc(arc, 180, 90);


        arc.X = rect.Right - d;
        path.AddArc(arc, 270, 90);


        arc.Y = rect.Bottom - d;
        path.AddArc(arc, 0, 90);


        arc.X = rect.Left;
        path.AddArc(arc, 90, 90);

        path.CloseFigure();
        return path;
    }

    private bool IsStatusColumn(int columnIndex)
    {
        if (columnIndex < 0 || columnIndex >= dgvTickets.Columns.Count) return false;
        var col = dgvTickets.Columns[columnIndex];
        return string.Equals(col.DataPropertyName, "Status", StringComparison.OrdinalIgnoreCase)
               || string.Equals(col.HeaderText, "Status", StringComparison.OrdinalIgnoreCase)
               || string.Equals(col.Name, "Status", StringComparison.OrdinalIgnoreCase)
               || string.Equals(col.Name, "colStatus", StringComparison.OrdinalIgnoreCase);
    }

    private bool IsTriagingColumn(int columnIndex)
    {
        if (columnIndex < 0 || columnIndex >= dgvTickets.Columns.Count) return false;
        var col = dgvTickets.Columns[columnIndex];
        return string.Equals(col.DataPropertyName, "Triaging", StringComparison.OrdinalIgnoreCase)
               || string.Equals(col.HeaderText, "Triagem", StringComparison.OrdinalIgnoreCase)
               || string.Equals(col.Name, "Triaging", StringComparison.OrdinalIgnoreCase);
    }

    private DataGridViewColumn? FindStatusColumn()
        => dgvTickets.Columns
            .Cast<DataGridViewColumn>()
            .FirstOrDefault(c =>
                string.Equals(c.DataPropertyName, "Status", StringComparison.OrdinalIgnoreCase)
                || string.Equals(c.HeaderText, "Status", StringComparison.OrdinalIgnoreCase)
                || string.Equals(c.Name, "Status", StringComparison.OrdinalIgnoreCase)
                || string.Equals(c.Name, "colStatus", StringComparison.OrdinalIgnoreCase));


}
