using System.Linq;
using System.Windows.Forms;
using System.Drawing;

namespace PayHelp.Desktop.WinForms;

public partial class FrmHome : Form
{
    private readonly SessionContext _session;
    private readonly IServiceProvider _provider;

    public FrmHome(SessionContext session, IServiceProvider provider)
    {
        _session = session;
        _provider = provider;
        InitializeComponent();
        Theme.Apply(this);

        BuildHomeLikeWeb();
    }

    private void BuildHomeLikeWeb()
    {

        this.Controls.Clear();
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.MinimumSize = new Size(900, 600);


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
            Text = "PayHelp",
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

        lblUser.ForeColor = Color.White;
        lblUser.AutoSize = true;
        lblUser.Margin = new Padding(0, 6, 12, 0);


        btnSair.Tag = "outline";
        btnSair.Text = "Sair";
        btnSair.Height = 36;
        btnSair.MinimumSize = new Size(80, 36);

        navRight.Controls.Add(lblUser);
        navRight.Controls.Add(btnSair);

        navbar.Controls.Add(navRight);
        navbar.Controls.Add(navLeft);


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
            Text = "PayHelp",
            Font = new Font("Segoe UI Light", 36f, FontStyle.Regular),
            AutoSize = true,
            Anchor = AnchorStyles.None
        };
        var subtitle = new Label
        {
            Text = "Bem-vindo ao sistema de suporte.",
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


        ConfigureHeroButton(btnPainelSuporte, "Dashboard de Suporte");
        ConfigureHeroButton(btnMensagens, "Mensagens Automáticas");
        ConfigureHeroButton(btnRelatorios, "Relatórios");
        ConfigureHeroButton(btnPainelUsuario, "Painel do Usuário");
        ConfigureHeroButton(btnChat, "Chat");


        bool isLogged = _session.CurrentUser != null;
        bool isSupport = _session.IsSupport;

        lblUser.Text = _session.CurrentUser is null
            ? "Não autenticado"
            : $"Olá, {_session.CurrentUser.Nome}";


        foreach (var b in new[] { btnPainelUsuario, btnPainelSuporte, btnChat, btnMensagens, btnRelatorios })
            b.Parent = null;

        buttonsRow.Controls.Clear();
        if (isSupport)
        {

            ConfigureHeroButton(btnPainelSuporte, "Dashboard");
            ConfigureHeroButton(btnMensagens, "Mensagens Ativas");
            ConfigureHeroButton(btnRelatorios, "Relatórios");

            buttonsRow.Controls.Add(btnPainelSuporte);
            buttonsRow.Controls.Add(btnMensagens);
            buttonsRow.Controls.Add(btnRelatorios);
        }
        else
        {

            var btnAbrirChamado = new Button();
            ConfigureHeroButton(btnAbrirChamado, "Abrir Chamado");
            btnAbrirChamado.Tag = "outline";
            btnAbrirChamado.Click += (_, __) =>
            {
                if (_provider.GetService(typeof(FrmAbrirChamadoWizard)) is FrmAbrirChamadoWizard wiz)
                {
                    wiz.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Wizard de abertura não disponível nesta build.");
                }
            };
            buttonsRow.Controls.Add(btnAbrirChamado);

            ConfigureHeroButton(btnPainelUsuario, "Meus Chamados");
            buttonsRow.Controls.Add(btnPainelUsuario);
        }


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

    private void ConfigureHeroButton(Button btn, string text)
    {
        btn.Text = text;
        btn.Tag = "outline";
        btn.Width = 240;
        btn.Height = 44;
        btn.Margin = new Padding(12, 8, 12, 8);
        btn.Visible = true;
    }

    private void FrmHome_Load(object sender, EventArgs e)
    {

    }

    private void btnSair_Click(object sender, EventArgs e)
    {
        _session.CurrentUser = null;
        _session.AccessToken = null;
        Close();
    }

    private void btnPainelUsuario_Click(object sender, EventArgs e)
    {
        var f = (FrmPainelUsuario)_provider.GetService(typeof(FrmPainelUsuario))!;
        f.ShowDialog();
    }

    private void btnPainelSuporte_Click(object sender, EventArgs e)
    {
        var f = (FrmPainelSuporte)_provider.GetService(typeof(FrmPainelSuporte))!;
        f.ShowDialog();
    }

    private void btnChat_Click(object sender, EventArgs e)
    {
        if (_session.IsSupport)
        {
            return;
        }
        var f = (FrmChatChamado)_provider.GetService(typeof(FrmChatChamado))!;
        f.ShowDialog();
    }

    private void btnMensagens_Click(object sender, EventArgs e)
    {
        var f = (FrmMensagensAutomaticas)_provider.GetService(typeof(FrmMensagensAutomaticas))!;
        f.ShowDialog();
    }

    private void btnRelatorios_Click(object sender, EventArgs e)
    {
        var f = (FrmRelatorios)_provider.GetService(typeof(FrmRelatorios))!;
        f.ShowDialog();
    }
}
