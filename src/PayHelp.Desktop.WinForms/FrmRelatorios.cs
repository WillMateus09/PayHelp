using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PayHelp.Desktop.WinForms
{
    public partial class FrmRelatorios : Form
    {
        private readonly ApiClient _api;

        private Label? lblResumoTotal;
        private Label? lblResumoEncerrados;
        private Label? lblResumoEmAtendimento;
        private Label? lblResumoAbertos;
        private Label? lblResumoMedia;
        private Label? lblResumoTaxa;

        public FrmRelatorios(ApiClient api)
        {
            _api = api;
            InitializeComponent();
            Theme.Apply(this);
            TicketStatus.Bind(cmbStatus);
            dgv.CellPainting += Dgv_CellPainting;
        }

        private async void btnGerar_Click(object sender, EventArgs e)
        {
            using var _ = LoadingOverlay.Show(this, "Gerando relatório...");
            DateTime? de = chkDe.Checked ? dtpDe.Value.ToUniversalTime() : (DateTime?)null;
            DateTime? ate = chkAte.Checked ? dtpAte.Value.ToUniversalTime() : (DateTime?)null;
            string? status = string.IsNullOrWhiteSpace(cmbStatus.Text) ? null : cmbStatus.Text;
            var list = await _api.GerarRelatorioAsync(de, ate, status);
            dgv.DataSource = list ?? new List<ApiClient.RelatorioDto>();
            AplicarEstiloGrid();
            AtualizarResumo(list);
        }

        private void AplicarEstiloGrid()
        {
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgv.RowTemplate.Height = 40;
            dgv.DefaultCellStyle.Padding = new Padding(0, 6, 0, 6);
            dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
        }

        private void Dgv_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            var grid = (DataGridView)sender!;
            var col = grid.Columns[e.ColumnIndex];
            if (!string.Equals(col.HeaderText, "Status", StringComparison.OrdinalIgnoreCase)) return;
            e.Handled = true;
            e.PaintBackground(e.ClipBounds, true);
            string? text = e.FormattedValue?.ToString();
            if (string.IsNullOrEmpty(text)) return;
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
            int padH = 8;
            int padV = 4;
            int badgeWidth = Math.Min(e.CellBounds.Width - 10, textSize.Width + padH * 2);
            int badgeHeight = textSize.Height + padV * 2;
            int x = e.CellBounds.X + (e.CellBounds.Width - badgeWidth) / 2;
            int y = e.CellBounds.Y + (e.CellBounds.Height - badgeHeight) / 2;
            var badgeRect = new Rectangle(x, y, badgeWidth, badgeHeight);
            using (var path = RoundedRect(badgeRect, 12))
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

        private void AtualizarResumo(List<ApiClient.RelatorioDto>? list)
        {
            list ??= new List<ApiClient.RelatorioDto>();
            int total = list.Count;
            int encerrados = list.Count(i => string.Equals(i.Status, "Encerrado", StringComparison.OrdinalIgnoreCase));
            int emAtendimento = list.Count(i => string.Equals(i.Status, "EmAtendimento", StringComparison.OrdinalIgnoreCase) || string.Equals(i.Status, "Em Atendimento", StringComparison.OrdinalIgnoreCase));
            int abertos = Math.Max(0, total - (encerrados + emAtendimento));

            var duracoes = new List<TimeSpan>();
            foreach (var i in list)
            {
                if (i.Duracao.HasValue)
                    duracoes.Add(i.Duracao.Value);
            }
            double mediaHoras = duracoes.Count > 0 ? duracoes.Average(d => d.TotalHours) : 0.0;
            double taxa = total > 0 ? (double)encerrados / total * 100.0 : 0.0;

            if (lblResumoTotal != null) lblResumoTotal.Text = $"Total: {total}";
            if (lblResumoEncerrados != null) lblResumoEncerrados.Text = $"Encerrados: {encerrados}";
            if (lblResumoEmAtendimento != null) lblResumoEmAtendimento.Text = $"Em Atendimento: {emAtendimento}";
            if (lblResumoAbertos != null) lblResumoAbertos.Text = $"Abertos: {abertos}";
            if (lblResumoMedia != null) lblResumoMedia.Text = $"Tempo Médio: {mediaHoras:F1} h";
            if (lblResumoTaxa != null) lblResumoTaxa.Text = $"Taxa de Resolução: {taxa:F1}%";
        }
    }
}
