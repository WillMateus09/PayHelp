using System;
using System.Windows.Forms;
using System.Drawing;

namespace PayHelp.Desktop.WinForms;

/// <summary>
/// Modal customizado para coletar feedback do usuário quando marca um chamado como resolvido
/// </summary>
public class FeedbackDialog : Form
{
    private readonly TextBox txtFeedback;
    private readonly Panel pnlStars;
    private readonly Button btnConfirmar;
    private readonly Button btnCancelar;
    private int? _notaSelecionada;

    public int? NotaSelecionada => _notaSelecionada;
    public string? Feedback => string.IsNullOrWhiteSpace(txtFeedback.Text) ? null : txtFeedback.Text.Trim();

    public FeedbackDialog()
    {
        // Configurações do Form
        this.Text = "Feedback de Resolução";
        this.Size = new Size(500, 450);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.BackColor = Color.FromArgb(248, 249, 250);

        // Layout principal
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(20),
            BackColor = Color.FromArgb(248, 249, 250)
        };

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Título
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Avaliação
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Feedback
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 10)); // Espaço
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Botões

        // Título e descrição
        var lblTitulo = new Label
        {
            Text = "✅ Marcar como Resolvido",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.FromArgb(16, 185, 129),
            AutoSize = true,
            Padding = new Padding(0, 0, 0, 10)
        };

        var lblDescricao = new Label
        {
            Text = "Conte se a IA conseguiu te ajudar, se tem algo que poderia melhorar ou se o atendimento foi rápido.",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(107, 114, 128),
            AutoSize = true,
            MaximumSize = new Size(440, 0),
            Padding = new Padding(0, 0, 0, 15)
        };

        var pnlTitulo = new Panel { AutoSize = true, Dock = DockStyle.Top };
        pnlTitulo.Controls.Add(lblDescricao);
        pnlTitulo.Controls.Add(lblTitulo);
        lblDescricao.Top = lblTitulo.Height + 5;

        // Seção de avaliação por estrelas
        var lblAvaliacao = new Label
        {
            Text = "Como você avalia o atendimento?",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(31, 41, 55),
            AutoSize = true,
            Padding = new Padding(0, 10, 0, 10)
        };

        pnlStars = new Panel
        {
            Height = 50,
            Dock = DockStyle.Top
        };

        // Criar 5 estrelas
        for (int i = 1; i <= 5; i++)
        {
            var star = CriarEstrela(i);
            star.Left = (i - 1) * 45 + 100; // Centralizar
            star.Top = 5;
            pnlStars.Controls.Add(star);
        }

        var pnlAvaliacao = new Panel { AutoSize = true, Dock = DockStyle.Top };
        pnlAvaliacao.Controls.Add(pnlStars);
        pnlAvaliacao.Controls.Add(lblAvaliacao);
        lblAvaliacao.Dock = DockStyle.Top;

        // Campo de feedback
        var lblFeedback = new Label
        {
            Text = "Seu comentário (opcional):",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(31, 41, 55),
            AutoSize = true,
            Dock = DockStyle.Top,
            Padding = new Padding(0, 10, 0, 5)
        };

        txtFeedback = new TextBox
        {
            Multiline = true,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.White,
            Padding = new Padding(5),
            ScrollBars = ScrollBars.Vertical
        };

        var pnlFeedback = new Panel { Dock = DockStyle.Fill };
        pnlFeedback.Controls.Add(txtFeedback);
        pnlFeedback.Controls.Add(lblFeedback);
        lblFeedback.Dock = DockStyle.Top;

        // Botões
        btnConfirmar = new Button
        {
            Text = "✓ Confirmar Resolução",
            Width = 180,
            Height = 40,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(16, 185, 129),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnConfirmar.FlatAppearance.BorderSize = 0;
        btnConfirmar.Click += (s, e) =>
        {
            if (!_notaSelecionada.HasValue)
            {
                MessageBox.Show("Por favor, selecione uma avaliação (estrelas) antes de confirmar.", 
                    "Avaliação Obrigatória", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        };

        btnCancelar = new Button
        {
            Text = "Cancelar",
            Width = 120,
            Height = 40,
            Font = new Font("Segoe UI", 10),
            BackColor = Color.FromArgb(229, 231, 235),
            ForeColor = Color.FromArgb(75, 85, 99),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnCancelar.FlatAppearance.BorderSize = 0;
        btnCancelar.Click += (s, e) =>
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        };

        var pnlBotoes = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            AutoSize = true
        };
        pnlBotoes.Controls.Add(btnConfirmar);
        pnlBotoes.Controls.Add(btnCancelar);

        // Adicionar ao layout
        layout.Controls.Add(pnlTitulo, 0, 0);
        layout.Controls.Add(pnlAvaliacao, 0, 1);
        layout.Controls.Add(pnlFeedback, 0, 2);
        layout.Controls.Add(new Panel(), 0, 3); // Espaçador
        layout.Controls.Add(pnlBotoes, 0, 4);

        this.Controls.Add(layout);

        // Aplicar tema se disponível
        try { Theme.Apply(this); } catch { }
    }

    private Label CriarEstrela(int valor)
    {
        var star = new Label
        {
            Text = "★",
            Font = new Font("Segoe UI", 24),
            ForeColor = Color.FromArgb(209, 213, 219),
            Cursor = Cursors.Hand,
            Size = new Size(40, 40),
            TextAlign = ContentAlignment.MiddleCenter,
            Tag = valor
        };

        star.MouseEnter += (s, e) => HighlightStars(valor);
        star.MouseLeave += (s, e) => HighlightStars(_notaSelecionada ?? 0);
        star.Click += (s, e) =>
        {
            _notaSelecionada = valor;
            HighlightStars(valor);
        };

        return star;
    }

    private void HighlightStars(int upTo)
    {
        foreach (Control ctrl in pnlStars.Controls)
        {
            if (ctrl is Label star && star.Tag is int valor)
            {
                star.ForeColor = valor <= upTo 
                    ? Color.FromArgb(251, 191, 36) 
                    : Color.FromArgb(209, 213, 219);
            }
        }
    }
}
