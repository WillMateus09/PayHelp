using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PayHelp.Desktop.WinForms;

public static class GridStyling
{

    public static void ApplyTicketsGrid(DataGridView grid)
    {
        grid.AutoGenerateColumns = false;
        grid.Columns.Clear();

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Titulo",
            HeaderText = "Título",
            Name = "colTitulo",
            FillWeight = 180
        });

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Status",
            HeaderText = "Status",
            Name = "colStatus",
            FillWeight = 90
        });

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "CriadoEmUtc",
            HeaderText = "Criado em",
            Name = "colCriado",
            FillWeight = 110
        });

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "EncerradoEmUtc",
            HeaderText = "Encerrado em",
            Name = "colEncerrado",
            FillWeight = 110
        });

        AttachCommonEvents(grid, type: "tickets");
    }


    public static void ApplyRelatoriosGrid(DataGridView grid)
    {
        grid.AutoGenerateColumns = false;
        grid.Columns.Clear();

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "TicketId",
            HeaderText = "Ticket",
            Name = "colTicket",
            Visible = false
        });

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Status",
            HeaderText = "Status",
            Name = "colStatus",
            FillWeight = 90
        });

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "SolicitanteEmail",
            HeaderText = "E-mail",
            Name = "colEmail",
            FillWeight = 150
        });

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "SolicitanteRole",
            HeaderText = "Perfil",
            Name = "colPerfil",
            FillWeight = 80
        });

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Duracao",
            HeaderText = "Duração",
            Name = "colDuracao",
            FillWeight = 90
        });

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "CriadoEmUtc",
            HeaderText = "Criado em",
            Name = "colCriado",
            FillWeight = 110
        });

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "EncerradoEmUtc",
            HeaderText = "Encerrado em",
            Name = "colEncerrado",
            FillWeight = 110
        });

        AttachCommonEvents(grid, type: "relatorios");
    }

    private static void AttachCommonEvents(DataGridView grid, string type)
    {

        var mark = $"grid-{type}-styled";
        if (!Equals(grid.Tag as string, mark))
        {
            grid.CellFormatting += (s, e) =>
            {
                if (grid.Rows.Count == 0) return;
                if (e.RowIndex < 0 || e.RowIndex >= grid.Rows.Count) return;
                if (e.ColumnIndex < 0 || e.ColumnIndex >= grid.Columns.Count) return;
                if (e.Value is null) return;

                var colName = grid.Columns[e.ColumnIndex].Name;
                if (colName is "colCriado" or "colEncerrado")
                {
                    if (e.Value is DateTime dtUtc)
                    {
                        var local = DateTime.SpecifyKind(dtUtc, DateTimeKind.Utc).ToLocalTime();
                        e.Value = local.ToString("dd/MM HH:mm");
                        e.FormattingApplied = true;
                    }
                }
                else if (type == "relatorios" && colName == "colDuracao")
                {
                    if (e.Value is TimeSpan ts)
                    {
                        e.Value = ts.ToString(@"hh\:mm");
                        e.FormattingApplied = true;
                    }
                    else if (e.Value is null)
                    {
                        e.Value = "";
                        e.FormattingApplied = true;
                    }
                }
            };

            grid.CellPainting += (s, e) =>
            {
                if (grid.Rows.Count == 0) return;
                if (e.RowIndex < 0 || e.RowIndex >= grid.Rows.Count) return;
                if (e.ColumnIndex < 0 || e.ColumnIndex >= grid.Columns.Count) return;
                var col = grid.Columns[e.ColumnIndex];
                if (col.Name != "colStatus") return;

                var cell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex];
                var value = cell?.Value?.ToString() ?? "";
                if (string.IsNullOrWhiteSpace(value)) return;

                e.Handled = true;
                e.PaintBackground(e.ClipBounds, true);

                var chipRect = e.CellBounds;
                chipRect.Inflate(-6, -6);

                using var path = GetRoundRect(chipRect, 10);
                var (bg, fg) = StatusColors(value);
                using var b = new SolidBrush(bg);
                using var p = new Pen(bg);
                if (e.Graphics is null) return;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillPath(b, path);
                e.Graphics.DrawPath(p, path);


                var font = e.CellStyle?.Font ?? grid.Font;
                TextRenderer.DrawText(
                    e.Graphics,
                    value,
                    font,
                    chipRect,
                    fg,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis
                );
            };

            grid.Tag = mark;
        }
    }

    private static (Color bg, Color fg) StatusColors(string status)
    {


        status = status?.Trim() ?? "";
        if (status.Equals(TicketStatus.Aberto, StringComparison.OrdinalIgnoreCase))
            return (Color.FromArgb(230, 240, 252), Color.FromArgb(40, 120, 214));
        if (status.Equals(TicketStatus.EmAtendimento, StringComparison.OrdinalIgnoreCase))
            return (Color.FromArgb(219, 242, 239), Color.FromArgb(0, 171, 143));
        if (status.Equals(TicketStatus.Encerrado, StringComparison.OrdinalIgnoreCase))
            return (Color.FromArgb(253, 231, 231), Color.FromArgb(220, 76, 70));

        return (Color.FromArgb(235, 242, 255), Color.FromArgb(40, 120, 214));
    }

    private static GraphicsPath GetRoundRect(Rectangle r, int radius)
    {
        int d = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
