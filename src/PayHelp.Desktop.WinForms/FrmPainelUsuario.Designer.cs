using System;
using System.Windows.Forms;

namespace PayHelp.Desktop.WinForms
{
    partial class FrmPainelUsuario
    {
        private System.ComponentModel.IContainer components = null;
        private DataGridView dgvTickets;
        private Button btnRecarregar;
        private Button btnAbrirChat;
        private Button btnNovo;
        private TextBox txtTitulo;
        private TextBox txtDesc;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.dgvTickets = new System.Windows.Forms.DataGridView();
            this.btnRecarregar = new System.Windows.Forms.Button();
            this.btnAbrirChat = new System.Windows.Forms.Button();
            this.btnNovo = new System.Windows.Forms.Button();
            this.txtTitulo = new System.Windows.Forms.TextBox();
            this.txtDesc = new System.Windows.Forms.TextBox();
            var topPanel = new System.Windows.Forms.FlowLayoutPanel();
            var openPanel = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTickets)).BeginInit();
            this.SuspendLayout();


            this.dgvTickets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvTickets.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvTickets.MultiSelect = false;
            this.dgvTickets.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvTickets.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.DgvTickets_CellPainting);
            this.dgvTickets.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.DgvTickets_CellFormatting);
            this.dgvTickets.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvTickets_CellDoubleClick);


            topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            topPanel.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            topPanel.Padding = new System.Windows.Forms.Padding(8);
            topPanel.AutoSize = true;


            this.btnRecarregar.Text = "Recarregar";
            this.btnRecarregar.Click += new System.EventHandler(this.btnRecarregar_Click);
            this.btnAbrirChat.Text = "Abrir Chat";
            this.btnAbrirChat.Click += new System.EventHandler(this.btnAbrirChat_Click);
            this.btnNovo.Text = "Novo";
            this.btnNovo.Click += new System.EventHandler(this.btnNovo_Click);


            openPanel.ColumnCount = 2; openPanel.RowCount = 2;
            openPanel.AutoSize = true;
            openPanel.Controls.Add(new Label { Text = "Título:", AutoSize = true }, 0, 0);
            openPanel.Controls.Add(this.txtTitulo, 1, 0);
            openPanel.Controls.Add(new Label { Text = "Descrição:", AutoSize = true }, 0, 1);
            openPanel.Controls.Add(this.txtDesc, 1, 1);
            this.txtTitulo.Width = 240; this.txtDesc.Width = 320;

            topPanel.Controls.Add(this.btnRecarregar);
            topPanel.Controls.Add(this.btnAbrirChat);
            topPanel.Controls.Add(this.btnNovo);
            topPanel.Controls.Add(openPanel);


            this.Text = "Painel do Usuário";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.MinimumSize = new System.Drawing.Size(800, 480);
            this.Controls.Add(this.dgvTickets);
            this.Controls.Add(topPanel);

            ((System.ComponentModel.ISupportInitialize)(this.dgvTickets)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
