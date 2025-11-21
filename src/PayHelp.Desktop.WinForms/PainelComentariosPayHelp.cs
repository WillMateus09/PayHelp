using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PayHelp.Desktop.WinForms
{
    /// <summary>
    /// Painel lateral para exibir comentários e avaliações dos usuários no dashboard do suporte
    /// </summary>
    public partial class PainelComentariosPayHelp : UserControl
    {
        private ListView lstComentarios;
        private Label lblTitulo;
        private Label lblSemDados;

        public PainelComentariosPayHelp()
        {
            InitializeComponent();
            ConfigurarInterface();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(350, 500);
            this.BackColor = Color.White;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.Padding = new Padding(10);
        }

        private void ConfigurarInterface()
        {
            // Título
            lblTitulo = new Label
            {
                Text = "💬 Comentários do PayHelp",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Theme.PrimaryColor,
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5)
            };
            this.Controls.Add(lblTitulo);

            // Lista de comentários
            lstComentarios = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                Font = new Font("Segoe UI", 9),
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                BorderStyle = BorderStyle.None
            };

            lstComentarios.Columns.Add("Data", 90);
            lstComentarios.Columns.Add("Usuário", 120);
            lstComentarios.Columns.Add("Nota", 60);
            lstComentarios.Columns.Add("Feedback", 200);

            lstComentarios.DoubleClick += LstComentarios_DoubleClick;

            this.Controls.Add(lstComentarios);

            // Label para quando não houver dados
            lblSemDados = new Label
            {
                Text = "Nenhum feedback recebido ainda.",
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = Color.Gray,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };
            this.Controls.Add(lblSemDados);
        }

        public void CarregarComentarios(System.Collections.Generic.List<FeedbackEntryDto> feedbacks)
        {
            lstComentarios.Items.Clear();

            if (feedbacks == null || !feedbacks.Any())
            {
                lblSemDados.Visible = true;
                lstComentarios.Visible = false;
                return;
            }

            lblSemDados.Visible = false;
            lstComentarios.Visible = true;

            foreach (var f in feedbacks.OrderByDescending(f => f.DataUtc))
            {
                var item = new ListViewItem(f.DataUtc.ToLocalTime().ToString("dd/MM HH:mm"));
                item.SubItems.Add(string.IsNullOrWhiteSpace(f.NomeUsuario) ? f.EmailUsuario : f.NomeUsuario);
                item.SubItems.Add(FormatarEstrelas(f.Nota));
                item.SubItems.Add(TruncarTexto(f.Feedback, 80));

                // Cor baseada na nota
                if (f.Nota >= 5)
                    item.BackColor = Color.FromArgb(220, 255, 220);
                else if (f.Nota >= 3)
                    item.BackColor = Color.FromArgb(255, 255, 220);
                else
                    item.BackColor = Color.FromArgb(255, 220, 220);

                item.Tag = f;
                lstComentarios.Items.Add(item);
            }

            // Auto-ajustar colunas
            lstComentarios.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private string FormatarEstrelas(double nota)
        {
            int estrelas = (int)Math.Round(nota);
            return new string('★', estrelas) + new string('☆', 5 - estrelas) + $" ({nota:F1})";
        }

        private string TruncarTexto(string texto, int maxLength)
        {
            if (string.IsNullOrEmpty(texto))
                return "-";
            
            if (texto.Length <= maxLength)
                return texto;
            
            return texto.Substring(0, maxLength - 3) + "...";
        }

        private void LstComentarios_DoubleClick(object? sender, EventArgs e)
        {
            if (lstComentarios.SelectedItems.Count == 0) return;
            var sel = lstComentarios.SelectedItems[0].Tag as FeedbackEntryDto;
            if (sel == null) return;
            try
            {
                using var dlg = new FeedbackDetalheDialog(sel);
                dlg.ShowDialog(this);
            }
            catch { }
        }
    }

    /// <summary>
    /// DTO agregado antigo (mantido para compatibilidade, não usado agora)
    /// </summary>
    public class FeedbackUsuarioDto
    {
        public Guid UserId { get; set; }
        public string NomeUsuario { get; set; } = string.Empty;
        public string EmailUsuario { get; set; } = string.Empty;
        public double MediaNotas { get; set; }
        public int UltimaNota { get; set; }
        public string UltimoFeedback { get; set; } = string.Empty;
        public int TotalChamados { get; set; }
    }

    /// <summary>
    /// DTO para cada entrada individual de feedback
    /// </summary>
    public class FeedbackEntryDto
    {
        public Guid TicketId { get; set; }
        public string NomeUsuario { get; set; } = string.Empty;
        public string EmailUsuario { get; set; } = string.Empty;
        public int Nota { get; set; }
        public string Feedback { get; set; } = string.Empty;
        public DateTime DataUtc { get; set; }
    }
}
