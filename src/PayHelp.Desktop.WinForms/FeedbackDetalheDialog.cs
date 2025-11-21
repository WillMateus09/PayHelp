using System;
using System.Drawing;
using System.Windows.Forms;

namespace PayHelp.Desktop.WinForms
{
    public class FeedbackDetalheDialog : Form
    {
        private readonly FeedbackEntryDto _entry;

        public FeedbackDetalheDialog(FeedbackEntryDto entry)
        {
            _entry = entry;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Detalhes do Feedback";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.ClientSize = new Size(560, 380);
            this.Padding = new Padding(12);

            var lblHeader = new Label
            {
                Dock = DockStyle.Top,
                AutoSize = false,
                Height = 60,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Text = $"Usuário: {(!string.IsNullOrWhiteSpace(_entry.NomeUsuario) ? _entry.NomeUsuario : _entry.EmailUsuario)}\nData: {_entry.DataUtc.ToLocalTime():dd/MM/yyyy HH:mm}  •  Ticket: #{_entry.TicketId.ToString()[..8]}"
            };

            var lblNota = new Label
            {
                Dock = DockStyle.Top,
                AutoSize = false,
                Height = 32,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.DimGray,
                Text = FormatarEstrelas(_entry.Nota)
            };

            var txtFeedback = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 10),
                Text = string.IsNullOrWhiteSpace(_entry.Feedback) ? "(sem texto de feedback)" : _entry.Feedback
            };

            var btnFechar = new Button
            {
                Dock = DockStyle.Bottom,
                Text = "Fechar",
                Height = 34
            };
            btnFechar.Click += (_, __) => this.Close();

            this.Controls.Add(txtFeedback);
            this.Controls.Add(btnFechar);
            this.Controls.Add(lblNota);
            this.Controls.Add(lblHeader);
            Theme.Apply(this);
        }

        private static string FormatarEstrelas(int nota)
        {
            int n = Math.Max(0, Math.Min(5, nota));
            return new string('★', n) + new string('☆', 5 - n) + $" ({n}/5)";
        }
    }
}
