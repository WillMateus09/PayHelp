using System.Drawing;
using System.Windows.Forms;

namespace PayHelp.Desktop.WinForms;

public sealed class LoadingOverlay : IDisposable
{
    private readonly Control _parent;
    private readonly Panel _overlay;
    private readonly Cursor _oldCursor;

    private LoadingOverlay(Control parent, string text)
    {
        _parent = parent;


        _overlay = new Panel
        {
            BackColor = Color.FromArgb(120, 245, 246, 248),
            Dock = DockStyle.Fill
        };
        _overlay.CreateControl();

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(12)
        };
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.BackColor = Color.Transparent;

        var bar = new ProgressBar
        {
            Style = ProgressBarStyle.Marquee,
            MarqueeAnimationSpeed = 32,
            Width = 220,
            Height = 22,
            Anchor = AnchorStyles.None
        };
        var lbl = new Label
        {
            Text = string.IsNullOrWhiteSpace(text) ? "Carregando..." : text,
            AutoSize = true,
            ForeColor = Color.FromArgb(20, 24, 28),
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleCenter,
            Padding = new Padding(0, 8, 0, 0)
        };

        var center = new Panel
        {
            Dock = DockStyle.None,
            AutoSize = true
        };
        center.Controls.Add(bar);
        lbl.Top = bar.Bottom + 6;
        lbl.Left = (bar.Width - lbl.Width) / 2;
        center.Controls.Add(lbl);


        _overlay.Controls.Add(center);
        _overlay.Resize += (_, __) =>
        {
            center.Left = (_overlay.Width - center.Width) / 2;
            center.Top = (_overlay.Height - center.Height) / 2;
        };


        _overlay.Enabled = true;


        _parent.Controls.Add(_overlay);
        _overlay.BringToFront();


    _oldCursor = Cursor.Current ?? Cursors.Default;
        Cursor.Current = Cursors.WaitCursor;
        _parent.UseWaitCursor = true;
        foreach (Control c in _parent.Controls) c.UseWaitCursor = true;
    }

    public static LoadingOverlay Show(Control parent, string? text = null)
        => new LoadingOverlay(parent, text ?? "Carregando...");

    public void Dispose()
    {
        try
        {
            if (!_overlay.IsDisposed && _parent.Controls.Contains(_overlay))
            {
                _parent.Controls.Remove(_overlay);
                _overlay.Dispose();
            }
        }
        catch {  }
        finally
        {
            Cursor.Current = _oldCursor;
            _parent.UseWaitCursor = false;
            foreach (Control c in _parent.Controls) c.UseWaitCursor = false;
        }
    }
}
