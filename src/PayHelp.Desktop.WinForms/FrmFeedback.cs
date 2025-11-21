using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PayHelp.Desktop.WinForms
{
    public partial class FrmFeedback : Form
    {
        private int _notaSelecionada = 0;
        private readonly PictureBox[] _estrelas = new PictureBox[5];
        private TextBox? txtComentario;
        private Button? btnConfirmar;
        private Button? btnCancelar;

        public int NotaSelecionada => _notaSelecionada;
        public string Comentario => txtComentario?.Text.Trim() ?? string.Empty;

        public FrmFeedback()
        {
            InitializeComponent();
            ConfigurarFormulario();
            CriarInterface();
            Theme.Apply(this);
        }

        private void ConfigurarFormulario()
        {
            this.Text = "Avaliação do Atendimento";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(500, 480);
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

            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 180));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Título
            var lblTitulo = new Label
            {
                Text = "Como foi a resolução do seu chamado?",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 45, 45),
                AutoSize = true,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0, 0, 0, 8)
            };

            // Painel de Estrelas
            var painelEstrelas = CriarPainelEstrelas();

            // Seção de Comentário
            var secaoComentario = CriarSecaoComentario();

            // Botões
            var painelBotoes = CriarPainelBotoes();

            mainPanel.Controls.Add(lblTitulo, 0, 0);
            mainPanel.Controls.Add(new Panel { Height = 1 }, 0, 1);
            mainPanel.Controls.Add(painelEstrelas, 0, 2);
            mainPanel.Controls.Add(secaoComentario, 0, 3);
            mainPanel.Controls.Add(painelBotoes, 0, 4);

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

            // Calcular posição centralizada
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
                    Cursor = Cursors.Hand,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Margin = new Padding(espacamento / 2, 0, espacamento / 2, 0)
                };

                int nota = i + 1;
                pic.Tag = nota;
                pic.Click += Estrela_Click;
                pic.MouseEnter += Estrela_MouseEnter;
                pic.MouseLeave += Estrela_MouseLeave;
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
            bool preenchida = nota <= _notaSelecionada;
            bool hover = pic.ClientRectangle.Contains(pic.PointToClient(Cursor.Position));

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color corPreenchimento = preenchida ? Color.FromArgb(255, 193, 7) : Color.White;
            Color corBorda = preenchida ? Color.FromArgb(255, 160, 0) : Color.FromArgb(220, 220, 220);

            if (hover && !preenchida)
            {
                corPreenchimento = Color.FromArgb(255, 243, 224);
                corBorda = Color.FromArgb(255, 193, 7);
            }

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

        private void Estrela_Click(object? sender, EventArgs e)
        {
            if (sender is not PictureBox pic) return;
            _notaSelecionada = (int)(pic.Tag ?? 0);
            AtualizarEstrelas();
        }

        private void Estrela_MouseEnter(object? sender, EventArgs e)
        {
            if (sender is PictureBox pic)
            {
                pic.Invalidate();
            }
        }

        private void Estrela_MouseLeave(object? sender, EventArgs e)
        {
            if (sender is PictureBox pic)
            {
                pic.Invalidate();
            }
        }

        private void AtualizarEstrelas()
        {
            foreach (var pic in _estrelas)
            {
                pic.Invalidate();
            }
        }

        private Panel CriarSecaoComentario()
        {
            var painel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0, 16, 0, 16)
            };

            painel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            painel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var lblComentario = new Label
            {
                Text = "Comentário (opcional):",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(70, 70, 70),
                AutoSize = true,
                Dock = DockStyle.Top,
                Padding = new Padding(0, 0, 0, 6)
            };

            txtComentario = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5F),
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.FixedSingle,
                MaxLength = 500
            };

            painel.Controls.Add(lblComentario, 0, 0);
            painel.Controls.Add(txtComentario, 0, 1);

            return painel;
        }

        private Panel CriarPainelBotoes()
        {
            var painel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                Dock = DockStyle.Bottom,
                Padding = new Padding(0, 12, 0, 0)
            };

            btnConfirmar = new Button
            {
                Text = "Confirmar",
                Width = 110,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(33, 150, 83),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(8, 0, 0, 0),
                Tag = "success"
            };
            btnConfirmar.FlatAppearance.BorderSize = 0;
            btnConfirmar.Click += BtnConfirmar_Click;

            btnCancelar = new Button
            {
                Text = "Cancelar",
                Width = 110,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(224, 224, 224),
                ForeColor = Color.FromArgb(60, 60, 60),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                Cursor = Cursors.Hand,
                Tag = "secondary"
            };
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            painel.Controls.Add(btnConfirmar);
            painel.Controls.Add(btnCancelar);

            return painel;
        }

        private void BtnConfirmar_Click(object? sender, EventArgs e)
        {
            if (_notaSelecionada == 0)
            {
                MessageBox.Show(
                    "Por favor, selecione pelo menos 1 estrela para avaliar o atendimento.",
                    "Avaliação Obrigatória",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(500, 480);
            this.Name = "FrmFeedback";
            this.ResumeLayout(false);
        }
    }
}
