using System.Drawing;
using System.Windows.Forms;

namespace PayHelp.Desktop.WinForms;

public partial class FrmMasterHome : Form
{
    private readonly SessionContext _session;
    private readonly IServiceProvider _provider;

    public FrmMasterHome(SessionContext session, IServiceProvider provider)
    {
        _session = session;
        _provider = provider;
        InitializeComponent();
        Theme.Apply(this);

        BuildMasterHome();
    }

    private void BuildMasterHome()
    {
        this.Controls.Clear();
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.MinimumSize = new Size(900, 600);
        this.Text = "PayHelp - Admin Master";

        // Navbar
        var navbar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 56,
            BackColor = Color.FromArgb(33, 37, 41)
        };

        var navLeft = new FlowLayoutPanel
        {
            Dock = DockStyle.Left,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(16, 12, 0, 0),
            AutoSize = false,
            Width = 600,
            BackColor = Color.Transparent
        };
        var brand = new Label
        {
            Text = "PayHelp - Admin Master",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            AutoSize = true
        };
        navLeft.Controls.Add(brand);

        var navRight = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 10, 16, 0),
            AutoSize = true,
            BackColor = Color.Transparent,
            WrapContents = false
        };

        var lblUser = new Label
        {
            Text = $"Olá, {_session.CurrentUser?.Nome ?? "Admin"}",
            ForeColor = Color.White,
            AutoSize = true,
            Margin = new Padding(0, 6, 12, 0)
        };

        var btnSair = new Button
        {
            Text = "Sair",
            Tag = "outline",
            Height = 36,
            MinimumSize = new Size(80, 36)
        };
        btnSair.Click += BtnSair_Click;

        navRight.Controls.Add(lblUser);
        navRight.Controls.Add(btnSair);

        navbar.Controls.Add(navRight);
        navbar.Controls.Add(navLeft);

        // Content area
        var content = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(245, 246, 248) };
        var hero = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            Padding = new Padding(12)
        };
        hero.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
        hero.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        hero.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        hero.RowStyles.Add(new RowStyle(SizeType.Absolute, 16));
        hero.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        hero.RowStyles.Add(new RowStyle(SizeType.Percent, 80));

        var title = new Label
        {
            Text = "Painel de Administração",
            Font = new Font("Segoe UI Light", 36f, FontStyle.Regular),
            AutoSize = true,
            Anchor = AnchorStyles.None
        };
        var subtitle = new Label
        {
            Text = "Gerencie usuários e configurações do sistema",
            Font = new Font("Segoe UI", 12f, FontStyle.Regular),
            ForeColor = Color.FromArgb(80, 86, 96),
            AutoSize = true,
            Anchor = AnchorStyles.None
        };

        var hr = new Panel
        {
            Height = 1,
            Dock = DockStyle.Top,
            BackColor = Color.FromArgb(220, 223, 228),
            Margin = new Padding(120, 8, 120, 8)
        };

        var buttonsRow = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            Anchor = AnchorStyles.None,
            WrapContents = false,
            Padding = new Padding(0),
            Margin = new Padding(0)
        };

        // Botões administrativos
        var btnGerenciarUsuarios = new Button();
        ConfigureAdminButton(btnGerenciarUsuarios, "Gerenciar Usuários");
        btnGerenciarUsuarios.Click += BtnGerenciarUsuarios_Click;

        var btnConfiguracoes = new Button();
        ConfigureAdminButton(btnConfiguracoes, "Configurações do Sistema");
        btnConfiguracoes.Click += BtnConfiguracoes_Click;

        buttonsRow.Controls.Add(btnGerenciarUsuarios);
        buttonsRow.Controls.Add(btnConfiguracoes);

        var centerTitle = WrapCenter(title);
        var centerSub = WrapCenter(subtitle);
        var centerButtons = WrapCenter(buttonsRow);

        hero.Controls.Add(new Panel(), 0, 0);
        hero.Controls.Add(centerTitle, 0, 1);
        hero.Controls.Add(centerSub, 0, 2);
        hero.Controls.Add(hr, 0, 3);
        hero.Controls.Add(centerButtons, 0, 4);

        content.Controls.Add(hero);

        this.Controls.Add(navbar);
        this.Controls.Add(content);

        Theme.Apply(this);
    }

    private static Panel WrapCenter(Control child)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(0, 12, 0, 8),
            BackColor = Color.Transparent
        };

        child.Margin = new Padding(0);
        child.AutoSize = true;
        child.Top = panel.Padding.Top;
        panel.Controls.Add(child);
        panel.Resize += (_, __) =>
        {
            child.Left = Math.Max(0, (panel.Width - child.Width) / 2);
        };
        return panel;
    }

    private void ConfigureAdminButton(Button btn, string text)
    {
        btn.Text = text;
        btn.Tag = "primary";
        btn.Width = 280;
        btn.Height = 48;
        btn.Margin = new Padding(12, 8, 12, 8);
        btn.Font = new Font("Segoe UI", 11f, FontStyle.Regular);
        btn.Visible = true;
    }

    private void BtnSair_Click(object? sender, EventArgs e)
    {
        _session.CurrentUser = null;
        _session.AccessToken = null;
        Close();
    }

    private void BtnGerenciarUsuarios_Click(object? sender, EventArgs e)
    {
        var frm = (FrmAdminUsuarios)_provider.GetService(typeof(FrmAdminUsuarios))!;
        frm.ShowDialog(this);
    }

    private void BtnConfiguracoes_Click(object? sender, EventArgs e)
    {
        var frm = (FrmAdminConfiguracoes)_provider.GetService(typeof(FrmAdminConfiguracoes))!;
        frm.ShowDialog(this);
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
        this.AutoScaleDimensions = new SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(900, 600);
        this.Name = "FrmMasterHome";
        this.Text = "PayHelp - Admin Master";
        this.ResumeLayout(false);
    }
}
