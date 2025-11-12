using System.Windows.Forms;
namespace PayHelp.Desktop.WinForms;

public class PromptDialog : Form
{
    private readonly TextBox _txt;
    private readonly Button _ok;
    private readonly Button _cancel;
    public string? ResultText => _txt.Text;

    public PromptDialog(string titulo)
    {
        Text = titulo;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MinimizeBox = false; MaximizeBox = false; ShowInTaskbar = false;
        Width = 520; Height = 260;

        _txt = new TextBox { Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical };
        _ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Width = 100, Height = 32 };
        _cancel = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, Width = 100, Height = 32 };

        var footer = new TableLayoutPanel { Dock = DockStyle.Bottom, Height = 48, ColumnCount = 3, Padding = new Padding(12) };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        footer.Controls.Add(new Panel { Dock = DockStyle.Fill }, 0, 0);
        footer.Controls.Add(_cancel, 1, 0);
        footer.Controls.Add(_ok, 2, 0);

        Controls.Add(_txt);
        Controls.Add(footer);

        AcceptButton = _ok; CancelButton = _cancel;
    }
}
