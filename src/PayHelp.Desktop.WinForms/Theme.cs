using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;

namespace PayHelp.Desktop.WinForms;





public static class Theme
{

    private static readonly Color Background   = Color.FromArgb(245, 246, 248);
    private static readonly Color Surface      = Color.FromArgb(255, 255, 255);
    private static readonly Color Primary      = Color.FromArgb(13, 110, 253);
    private static readonly Color PrimaryDark  = Color.FromArgb(10, 88, 202);
    private static readonly Color PrimaryLight = Color.FromArgb(231, 241, 255);
    private static readonly Color Danger       = Color.FromArgb(220, 53, 69);
    private static readonly Color Success      = Color.FromArgb(33, 150, 83);
    private static readonly Color Text         = Color.FromArgb(33, 37, 41);
    private static readonly Color TextMuted    = Color.FromArgb(108, 117, 125);
    private static readonly Color Border       = Color.FromArgb(222, 226, 230);

    public static Color BackgroundColor => Background;
    public static Color PrimaryColor => Primary;
    public static Color SuccessColor => Success;
    public static Color TextColor => Text;


    private static readonly Font DefaultFont      = new Font("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point);
    private static readonly Font DefaultFontBold  = new Font("Segoe UI Semibold", 10f, FontStyle.Bold);


    private const int ButtonCornerRadius      = 8;
    private const int ButtonVerticalPadding   = 10;
    private const int ButtonHorizontalPadding = 16;
    private const int ButtonMinWidth          = 110;


    /// Aplica o tema moderno a um formulário e todos os seus controles filhos.

    public static void Apply(Form form)
    {
        if (form is null) return;


        form.AutoScaleMode = AutoScaleMode.Dpi;
        form.Font = DefaultFont;
        form.BackColor = Background;
        form.ForeColor = Text;
        form.Padding = new Padding(12);
        form.DoubleBuffered(true);

        StyleChildren(form);
    }

    private static void StyleChildren(Control parent)
    {
        foreach (Control c in parent.Controls)
        {

            if (c.Font == null || c.Font.Equals(SystemFonts.DefaultFont))
                c.Font = DefaultFont;

            switch (c)
            {
                case LinkLabel link:
                    link.LinkBehavior = LinkBehavior.HoverUnderline;
                    link.LinkColor = Primary;
                    link.ActiveLinkColor = PrimaryDark;
                    link.VisitedLinkColor = Primary;
                    link.UseCompatibleTextRendering = true;
                    break;

                case Label lbl:
                    lbl.ForeColor = Text;
                    lbl.UseCompatibleTextRendering = true;
                    break;

                case Button btn:
                    StyleButton(btn);
                    break;

                case TextBox tb:
                    StyleTextBox(tb);
                    break;

                case RichTextBox rtb:
                    rtb.BorderStyle = BorderStyle.FixedSingle;
                    rtb.BackColor = Surface;
                    rtb.ForeColor = Text;
                    break;

                case ComboBox cb:
                    cb.FlatStyle = FlatStyle.Flat;
                    cb.BackColor = Surface;
                    cb.ForeColor = Text;

                    var chc = TextRenderer.MeasureText("Ag", cb.Font).Height + 10;
                    if (cb.ItemHeight < chc - 6) cb.ItemHeight = chc - 6;
                    break;

                case ListBox lb:
                    lb.BorderStyle = BorderStyle.FixedSingle;
                    lb.BackColor = Surface;
                    lb.ForeColor = Text;
                    break;

                case GroupBox gb:
                    gb.ForeColor = Text;
                    gb.BackColor = Background;
                    break;

                case Panel p:
                    p.BackColor = Surface;
                    p.DoubleBuffered(true);
                    break;

                case TabControl tab:
                    tab.DrawMode = TabDrawMode.Normal;
                    tab.Appearance = TabAppearance.Normal;
                    tab.BackColor = Background;
                    tab.ForeColor = Text;
                    foreach (TabPage page in tab.TabPages)
                    {
                        page.BackColor = Background;
                        page.ForeColor = Text;
                    }
                    break;

                case DataGridView grid:
                    StyleGrid(grid);
                    break;
            }

            if (c.HasChildren)
                StyleChildren(c);
        }
    }

    private static void StyleButton(Button btn)
    {
        btn.FlatStyle = FlatStyle.Flat;
        btn.Cursor = Cursors.Hand;
        btn.UseVisualStyleBackColor = false;
        btn.UseCompatibleTextRendering = true;
        btn.TextAlign = ContentAlignment.MiddleCenter;
        btn.AutoSize = false;
        btn.Font = DefaultFontBold;


        var textSize = TextRenderer.MeasureText("Ag", btn.Font);
        int targetHeight = textSize.Height + (ButtonVerticalPadding * 2);
        if (btn.Height < targetHeight) btn.Height = targetHeight;
        if (btn.MinimumSize.Height < targetHeight) btn.MinimumSize = new Size(ButtonMinWidth, targetHeight);

        btn.Padding = new Padding(ButtonHorizontalPadding, ButtonVerticalPadding, ButtonHorizontalPadding, ButtonVerticalPadding);
        btn.Margin = new Padding(8);
        if (btn.MinimumSize.Width < ButtonMinWidth) btn.MinimumSize = new Size(ButtonMinWidth, btn.MinimumSize.Height);


        btn.SetRoundedRegion(ButtonCornerRadius);

        var role = (btn.Tag as string)?.Trim().ToLowerInvariant();

        if (role == "secondary" || role == "outline")
        {

            btn.BackColor = Surface;
            btn.ForeColor = Primary;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Primary;

            btn.MouseEnter += (_, __) => btn.BackColor = PrimaryLight;
            btn.MouseLeave += (_, __) => btn.BackColor = Surface;
            btn.MouseDown += (_, __) => btn.BackColor = PrimaryLight;
            btn.MouseUp   += (_, __) => btn.BackColor = Surface;
        }
        else if (role == "danger")
        {
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Danger;
            btn.ForeColor = Color.White;

            btn.MouseEnter += (_, __) => btn.BackColor = Color.FromArgb(200, 35, 51);
            btn.MouseLeave += (_, __) => btn.BackColor = Danger;
            btn.MouseDown  += (_, __) => btn.BackColor = Color.FromArgb(173, 26, 43);
            btn.MouseUp    += (_, __) => btn.BackColor = Danger;
        }
        else if (role == "success")
        {
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Success;
            btn.ForeColor = Color.White;

            btn.MouseEnter += (_, __) => btn.BackColor = Color.FromArgb(25, 135, 75);
            btn.MouseLeave += (_, __) => btn.BackColor = Success;
            btn.MouseDown  += (_, __) => btn.BackColor = Color.FromArgb(20, 120, 65);
            btn.MouseUp    += (_, __) => btn.BackColor = Success;
        }
        else
        {

            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Primary;
            btn.ForeColor = Color.White;

            btn.MouseEnter += (_, __) => btn.BackColor = PrimaryDark;
            btn.MouseLeave += (_, __) => btn.BackColor = Primary;
            btn.MouseDown  += (_, __) => btn.BackColor = PrimaryDark;
            btn.MouseUp    += (_, __) => btn.BackColor = Primary;
        }
    }

    private static void StyleTextBox(TextBox tb)
    {
        tb.BorderStyle = BorderStyle.FixedSingle;
        tb.BackColor = Surface;
        tb.ForeColor = Text;
        tb.Margin = new Padding(6);


        if (!tb.Multiline)
        {
            var h = TextRenderer.MeasureText("Ag", tb.Font).Height + 10;
            if (tb.Height < h) tb.Height = h;
        }
    }

    private static void StyleGrid(DataGridView grid)
    {
        grid.EnableHeadersVisualStyles = false;
        grid.BackgroundColor = Surface;
        grid.BorderStyle = BorderStyle.None;
        grid.GridColor = Border;
        grid.DoubleBuffered(true);


        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeColumns = false;
        grid.AllowUserToResizeRows = false;
        grid.AllowUserToOrderColumns = false;
        grid.MultiSelect = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        grid.ColumnHeadersDefaultCellStyle.BackColor = PrimaryLight;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Text;
        grid.ColumnHeadersDefaultCellStyle.Font = DefaultFontBold;
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Primary;
        grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;

        grid.DefaultCellStyle.BackColor = Surface;
        grid.DefaultCellStyle.ForeColor = Text;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(235, 242, 255);
        grid.DefaultCellStyle.SelectionForeColor = Text;

        grid.RowHeadersVisible = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
    }
}

internal static class ThemeExtensions
{
    public static void DoubleBuffered(this Control control, bool enable)
    {

        var prop = control.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        prop?.SetValue(control, enable, null);
    }

    public static void SetRoundedRegion(this Control control, int radius)
    {

        try
        {
            void Apply()
            {
                if (control.Width <= 0 || control.Height <= 0) return;

                using var path = new GraphicsPath();
                int d = radius * 2;
                Rectangle r = new Rectangle(0, 0, control.Width, control.Height);
                path.AddArc(r.X, r.Y, d, d, 180, 90);
                path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
                path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
                path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
                control.Region = new Region(path);
            }

            Apply();
            control.Resize -= Control_Resize;
            control.Resize += Control_Resize;
            void Control_Resize(object? s, System.EventArgs e) => Apply();
        }
        catch
        {

        }
    }
}
