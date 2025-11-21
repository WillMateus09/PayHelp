using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace PayHelp.Desktop.WinForms
{
    public partial class FrmListaFeedbacks : Form
    {
        private readonly ApiClient _api;
        private DataGridView dgvFeedbacks;
        private Button btnRecarregar;
        private Button btnFechar;

        public FrmListaFeedbacks(ApiClient api)
        {
            _api = api;
            InitializeComponent();
            ConfigurarFormulario();
            CriarInterface();
            Theme.Apply(this);
        }

        private void ConfigurarFormulario()
        {
            this.Text = "Todos os Feedbacks";
            this.Size = new Size(900, 609);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimizeBox = true;
            this.MaximizeBox = true;
        }

        private void CriarInterface()
        {
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(16)
            };

            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Título
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Grid
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Botões

            // Título
            var lblTitulo = new Label
            {
                Text = "Lista de Feedbacks dos Usuários",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 45, 45),
                AutoSize = true,
                Dock = DockStyle.Top,
                Padding = new Padding(0, 0, 0, 12)
            };

            // DataGridView
            dgvFeedbacks = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                GridColor = Color.FromArgb(224, 224, 224),
                CellBorderStyle = DataGridViewCellBorderStyle.Single,
                RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single,
                BorderStyle = BorderStyle.FixedSingle,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 40,
                RowTemplate = { Height = 36 },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(240, 240, 240),
                    ForeColor = Color.FromArgb(60, 60, 60),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Padding = new Padding(8, 8, 8, 8),
                    Alignment = DataGridViewContentAlignment.MiddleLeft
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.White,
                    ForeColor = Color.Black,
                    SelectionBackColor = Color.FromArgb(230, 237, 255),
                    SelectionForeColor = Color.Black,
                    Padding = new Padding(8, 6, 8, 6),
                    Font = new Font("Segoe UI", 9F),
                    Alignment = DataGridViewContentAlignment.MiddleLeft
                }
            };

            dgvFeedbacks.CellPainting += DgvFeedbacks_CellPainting;
            dgvFeedbacks.CellDoubleClick += DgvFeedbacks_CellDoubleClick;

            // Painel de botões
            var painelBotoes = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                Dock = DockStyle.Bottom,
                Padding = new Padding(0, 12, 0, 0)
            };

            btnFechar = new Button
            {
                Text = "Fechar",
                Width = 110,
                Height = 36,
                Margin = new Padding(0, 0, 0, 0),
                Tag = "secondary"
            };
            btnFechar.Click += (s, e) => this.Close();

            btnRecarregar = new Button
            {
                Text = "Recarregar",
                Width = 120,
                Height = 36,
                Margin = new Padding(0, 0, 12, 0),
                Tag = "primary"
            };
            btnRecarregar.Click += async (s, e) => await CarregarFeedbacksAsync();

            painelBotoes.Controls.Add(btnFechar);
            painelBotoes.Controls.Add(btnRecarregar);

            mainPanel.Controls.Add(lblTitulo, 0, 0);
            mainPanel.Controls.Add(dgvFeedbacks, 0, 1);
            mainPanel.Controls.Add(painelBotoes, 0, 2);

            this.Controls.Add(mainPanel);
            this.Load += async (_, __) => await CarregarFeedbacksAsync();
        }

        private async System.Threading.Tasks.Task CarregarFeedbacksAsync()
        {
            try
            {
                using var loading = LoadingOverlay.Show(this, "Carregando feedbacks...");
                var feedbacks = await _api.ListarTodosFeedbacksAsync();

                if (feedbacks == null || feedbacks.Count == 0)
                {
                    dgvFeedbacks.DataSource = null;
                    MessageBox.Show(
                        "Nenhum feedback encontrado.",
                        "Informação",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    return;
                }

                dgvFeedbacks.DataSource = feedbacks;

                // Configurar colunas
                if (dgvFeedbacks.Columns["TicketId"] != null)
                    dgvFeedbacks.Columns["TicketId"].Visible = false;

                if (dgvFeedbacks.Columns["TicketTitulo"] != null)
                {
                    dgvFeedbacks.Columns["TicketTitulo"].HeaderText = "Chamado";
                    dgvFeedbacks.Columns["TicketTitulo"].Width = 200;
                    dgvFeedbacks.Columns["TicketTitulo"].MinimumWidth = 150;
                }

                if (dgvFeedbacks.Columns["UsuarioNome"] != null)
                {
                    dgvFeedbacks.Columns["UsuarioNome"].HeaderText = "Usuário";
                    dgvFeedbacks.Columns["UsuarioNome"].Width = 140;
                    dgvFeedbacks.Columns["UsuarioNome"].MinimumWidth = 100;
                }

                if (dgvFeedbacks.Columns["UsuarioEmail"] != null)
                {
                    dgvFeedbacks.Columns["UsuarioEmail"].HeaderText = "E-mail";
                    dgvFeedbacks.Columns["UsuarioEmail"].Width = 180;
                    dgvFeedbacks.Columns["UsuarioEmail"].MinimumWidth = 140;
                }

                if (dgvFeedbacks.Columns["Nota"] != null)
                {
                    dgvFeedbacks.Columns["Nota"].HeaderText = "Avaliação";
                    dgvFeedbacks.Columns["Nota"].Width = 110;
                    dgvFeedbacks.Columns["Nota"].MinimumWidth = 110;
                    dgvFeedbacks.Columns["Nota"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }

                if (dgvFeedbacks.Columns["Comentario"] != null)
                {
                    dgvFeedbacks.Columns["Comentario"].HeaderText = "Comentário";
                    dgvFeedbacks.Columns["Comentario"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    dgvFeedbacks.Columns["Comentario"].MinimumWidth = 200;
                    dgvFeedbacks.Columns["Comentario"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                }

                // Ajustar altura das linhas para comentários longos
                dgvFeedbacks.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

                if (dgvFeedbacks.Columns["DataCriacao"] != null)
                {
                    dgvFeedbacks.Columns["DataCriacao"].HeaderText = "Data";
                    dgvFeedbacks.Columns["DataCriacao"].Width = 130;
                    dgvFeedbacks.Columns["DataCriacao"].MinimumWidth = 110;
                    dgvFeedbacks.Columns["DataCriacao"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erro ao carregar feedbacks: {ex.Message}",
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void DgvFeedbacks_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            var grid = (DataGridView)sender!;
            var col = grid.Columns[e.ColumnIndex];

            // Pintar estrelas na coluna Nota
            if (col.Name == "Nota")
            {
                e.Handled = true;
                e.PaintBackground(e.ClipBounds, true);

                if (e.Value != null && int.TryParse(e.Value.ToString(), out int nota))
                {
                    int starSize = 18;
                    int spacing = 3;
                    int totalWidth = (starSize * 5) + (spacing * 4);
                    int startX = e.CellBounds.X + (e.CellBounds.Width - totalWidth) / 2;
                    int startY = e.CellBounds.Y + (e.CellBounds.Height - starSize) / 2;

                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                    for (int i = 0; i < 5; i++)
                    {
                        int x = startX + (i * (starSize + spacing));
                        var rect = new Rectangle(x, startY, starSize, starSize);
                        bool preenchida = (i + 1) <= nota;

                        Color corPreenchimento = preenchida ? Color.FromArgb(255, 193, 7) : Color.White;
                        Color corBorda = preenchida ? Color.FromArgb(255, 160, 0) : Color.FromArgb(220, 220, 220);

                        var path = CriarEstrela(rect);

                        using (var brush = new SolidBrush(corPreenchimento))
                        {
                            e.Graphics.FillPath(brush, path);
                        }

                        using (var pen = new Pen(corBorda, 1.5f))
                        {
                            e.Graphics.DrawPath(pen, path);
                        }
                    }
                }
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

        private void DgvFeedbacks_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dgvFeedbacks.Rows[e.RowIndex];
            var ticketId = (Guid)row.Cells["TicketId"].Value;
            var nota = (int)row.Cells["Nota"].Value;
            var comentario = row.Cells["Comentario"].Value?.ToString() ?? "";
            var dataCriacao = (DateTime)row.Cells["DataCriacao"].Value;

            var feedback = new ApiClient.FeedbackDto(
                Guid.NewGuid(),
                ticketId,
                Guid.Empty,
                nota,
                comentario,
                dataCriacao
            );

            using var form = new FrmVisualizarFeedback(feedback);
            form.ShowDialog(this);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1000, 600);
            this.Name = "FrmListaFeedbacks";
            this.ResumeLayout(false);
        }
    }
}
