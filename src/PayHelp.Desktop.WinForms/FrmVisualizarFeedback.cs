using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PayHelp.Desktop.WinForms
{
    public partial class FrmVisualizarFeedback : Form
    {
        private readonly ApiClient.FeedbackDto _feedback;
        private readonly PictureBox[] _estrelas = new PictureBox[5];

        public FrmVisualizarFeedback(ApiClient.FeedbackDto feedback)
        {
            _feedback = feedback;
            System.Diagnostics.Debug.WriteLine($"[FrmVisualizarFeedback] Construtor: Nota={feedback.Nota}, Comentario='{feedback.Comentario}'");
            InitializeComponent();
            ConfigurarFormulario();
            CriarInterface();
            Theme.Apply(this);
        }

        private void ConfigurarFormulario()
        {
            this.Text = "Feedback do Usuário";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(480, 480);
            this.BackColor = SystemColors.Control;
        }

        private void CriarInterface()
        {
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(24, 20, 24, 20)
            };

            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Título
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Estrelas
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 10)); // Espaço
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Comentário
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Botão

            var lblTitulo = new Label
            {
                Text = "Avaliação do Atendimento",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 45, 45),
                AutoSize = true,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0, 0, 0, 8)
            };

            var painelEstrelas = CriarPainelEstrelas();
            var painelComentario = CriarPainelComentario();
            var painelBotao = CriarPainelBotao();

            mainPanel.Controls.Add(lblTitulo, 0, 0);
            mainPanel.Controls.Add(painelEstrelas, 0, 1);
            mainPanel.Controls.Add(new Panel { Height = 1 }, 0, 2);
            mainPanel.Controls.Add(painelComentario, 0, 3);
            mainPanel.Controls.Add(painelBotao, 0, 4);

            System.Diagnostics.Debug.WriteLine($"[FrmVisualizarFeedback] Controles adicionados ao mainPanel. Total: {mainPanel.Controls.Count}");

            this.Controls.Add(mainPanel);
        }

        private Panel CriarPainelEstrelas()
        {
            var painel = new Panel
            {
                Height = 80,
                Dock = DockStyle.Top
            };

            var flowPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Anchor = AnchorStyles.Top
            };

            int larguraEstrela = 48;
            int espacamento = 8;
            int larguraTotal = (larguraEstrela * 5) + (espacamento * 4);
            flowPanel.Left = (painel.Width - larguraTotal) / 2;
            flowPanel.Top = 10;

            for (int i = 0; i < 5; i++)
            {
                var pic = new PictureBox
                {
                    Width = larguraEstrela,
                    Height = larguraEstrela,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Margin = new Padding(espacamento / 2, 0, espacamento / 2, 0)
                };

                int nota = i + 1;
                pic.Tag = nota;
                pic.Paint += Estrela_Paint;

                _estrelas[i] = pic;
                flowPanel.Controls.Add(pic);
            }

            painel.Controls.Add(flowPanel);
            return painel;
        }

        private void Estrela_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not PictureBox pic) return;

            int nota = (int)(pic.Tag ?? 0);
            bool preenchida = nota <= _feedback.Nota;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color corPreenchimento = preenchida ? Color.FromArgb(255, 193, 7) : Color.White;
            Color corBorda = preenchida ? Color.FromArgb(255, 160, 0) : Color.FromArgb(220, 220, 220);

            var rect = new Rectangle(4, 4, pic.Width - 8, pic.Height - 8);
            var path = CriarEstrela(rect);

            using (var brush = new SolidBrush(corPreenchimento))
            {
                g.FillPath(brush, path);
            }

            using (var pen = new Pen(corBorda, 2))
            {
                g.DrawPath(pen, path);
            }
        }

        private GraphicsPath CriarEstrela(Rectangle bounds)
        {
            var path = new GraphicsPath();
            
            float cx = bounds.X + bounds.Width / 2f;
            float cy = bounds.Y + bounds.Height / 2f;
            float raioExterno = Math.Min(bounds.Width, bounds.Height) / 2f;
            float raioInterno = raioExterno * 0.4f;

            PointF[] pontos = new PointF[10];
            for (int i = 0; i < 10; i++)
            {
                double angulo = (Math.PI / 2) + (i * Math.PI / 5);
                float raio = (i % 2 == 0) ? raioExterno : raioInterno;
                pontos[i] = new PointF(
                    cx + (float)(Math.Cos(angulo) * raio),
                    cy - (float)(Math.Sin(angulo) * raio)
                );
            }

            path.AddPolygon(pontos);
            return path;
        }

        private Panel CriarPainelInformacoes()
        {
            var painel = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(0, 8, 0, 8)
            };

            var lblData = new Label
            {
                Text = $"Data: {DateTime.SpecifyKind(_feedback.CriadoEmUtc, DateTimeKind.Utc).ToLocalTime():dd/MM/yyyy HH:mm}",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(100, 100, 100),
                AutoSize = true,
                Dock = DockStyle.Top
            };

            painel.Controls.Add(lblData);
            return painel;
        }

        private Panel CriarPainelComentario()
        {
            var painel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0, 8, 0, 8)
            };

            painel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            painel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var lblComentario = new Label
            {
                Text = "Comentário:",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(70, 70, 70),
                AutoSize = true,
                Dock = DockStyle.Top,
                Padding = new Padding(0, 0, 0, 8)
            };

            var txtComentario = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5F),
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true,
                Text = string.IsNullOrWhiteSpace(_feedback.Comentario) ? "(sem comentário)" : _feedback.Comentario,
                BackColor = Color.FromArgb(250, 250, 250),
                MinimumSize = new Size(0, 120)
            };

            System.Diagnostics.Debug.WriteLine($"[FrmVisualizarFeedback] TextBox.Text definido como: '{txtComentario.Text}'");

            painel.Controls.Add(lblComentario, 0, 0);
            painel.Controls.Add(txtComentario, 0, 1);

            System.Diagnostics.Debug.WriteLine($"[FrmVisualizarFeedback] Painel comentário criado. Controles: {painel.Controls.Count}");

            return painel;
        }

        private Panel CriarPainelBotao()
        {
            var painel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                Dock = DockStyle.Bottom,
                Padding = new Padding(0, 12, 0, 0)
            };

            var btnFechar = new Button
            {
                Text = "Fechar",
                Width = 110,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(224, 224, 224),
                ForeColor = Color.FromArgb(60, 60, 60),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                Cursor = Cursors.Hand,
                Tag = "secondary"
            };
            btnFechar.FlatAppearance.BorderSize = 0;
            btnFechar.Click += (s, e) => this.Close();

            painel.Controls.Add(btnFechar);

            return painel;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(460, 380);
            this.Name = "FrmVisualizarFeedback";
            this.ResumeLayout(false);
        }
    }
}
