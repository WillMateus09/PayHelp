namespace PayHelp.Desktop.WinForms
{
    partial class FrmRelatorios
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblDe;
        private System.Windows.Forms.Label lblAte;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DateTimePicker dtpDe;
        private System.Windows.Forms.DateTimePicker dtpAte;
        private System.Windows.Forms.CheckBox chkDe;
        private System.Windows.Forms.CheckBox chkAte;
        private System.Windows.Forms.ComboBox cmbStatus;
        private System.Windows.Forms.Button btnGerar;
        private System.Windows.Forms.DataGridView dgv;

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
            this.lblDe = new System.Windows.Forms.Label();
            this.lblAte = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.dtpDe = new System.Windows.Forms.DateTimePicker();
            this.dtpAte = new System.Windows.Forms.DateTimePicker();
            this.chkDe = new System.Windows.Forms.CheckBox();
            this.chkAte = new System.Windows.Forms.CheckBox();
            this.cmbStatus = new System.Windows.Forms.ComboBox();
            this.btnGerar = new System.Windows.Forms.Button();
            this.dgv = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            this.SuspendLayout();

            const int RowHeight = 36;
            const int ButtonColumnWidth = 140;

            var topBar = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink,
                Padding = new System.Windows.Forms.Padding(12, 12, 12, 8)
            };

            var grid = new System.Windows.Forms.TableLayoutPanel
            {
                Dock = System.Windows.Forms.DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink,
                ColumnCount = 4,
                RowCount = 3
            };
            grid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            grid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            grid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            grid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, ButtonColumnWidth));

            grid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, RowHeight));
            grid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, RowHeight));
            grid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, RowHeight));


            this.lblDe.AutoSize = true;
            this.lblDe.Text = "De:";
            this.lblDe.Margin = new System.Windows.Forms.Padding(0, 8, 8, 0);

            this.chkDe.Text = "Usar";
            this.chkDe.AutoSize = true;
            this.chkDe.Margin = new System.Windows.Forms.Padding(0, 6, 8, 0);

            this.dtpDe.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dtpDe.Margin = new System.Windows.Forms.Padding(0, 4, 12, 0);

            grid.Controls.Add(this.lblDe, 0, 0);
            grid.Controls.Add(this.chkDe, 1, 0);
            grid.Controls.Add(this.dtpDe, 2, 0);


            this.lblAte.AutoSize = true;
            this.lblAte.Text = "Até:";
            this.lblAte.Margin = new System.Windows.Forms.Padding(0, 8, 8, 0);

            this.chkAte.Text = "Usar";
            this.chkAte.AutoSize = true;
            this.chkAte.Margin = new System.Windows.Forms.Padding(0, 6, 8, 0);

            this.dtpAte.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dtpAte.Margin = new System.Windows.Forms.Padding(0, 4, 12, 0);

            grid.Controls.Add(this.lblAte, 0, 1);
            grid.Controls.Add(this.chkAte, 1, 1);
            grid.Controls.Add(this.dtpAte, 2, 1);


            this.lblStatus.AutoSize = true;
            this.lblStatus.Text = "Status:";
            this.lblStatus.Margin = new System.Windows.Forms.Padding(0, 8, 8, 0);

            this.cmbStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbStatus.Margin = new System.Windows.Forms.Padding(0, 4, 12, 0);

            this.btnGerar.Text = "Gerar";
            this.btnGerar.AutoSize = false;
            this.btnGerar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnGerar.Margin = new System.Windows.Forms.Padding(0);
            this.btnGerar.Click += new System.EventHandler(this.btnGerar_Click);

            grid.Controls.Add(this.lblStatus, 0, 2);
            grid.Controls.Add(this.cmbStatus, 2, 2);
            grid.Controls.Add(this.btnGerar, 3, 2);

            topBar.Controls.Add(grid);


            var resumoPanel = new System.Windows.Forms.TableLayoutPanel
            {
                Dock = System.Windows.Forms.DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink,
                ColumnCount = 4,
                RowCount = 2,
                Padding = new System.Windows.Forms.Padding(12, 0, 12, 8)
            };
            resumoPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            resumoPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            resumoPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            resumoPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            resumoPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28));
            resumoPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28));

            this.lblResumoTotal = new System.Windows.Forms.Label { AutoSize = true, Text = "Total: 0", Margin = new System.Windows.Forms.Padding(0, 0, 8, 0) };
            this.lblResumoEncerrados = new System.Windows.Forms.Label { AutoSize = true, Text = "Encerrados: 0", Margin = new System.Windows.Forms.Padding(0, 0, 8, 0) };
            this.lblResumoEmAtendimento = new System.Windows.Forms.Label { AutoSize = true, Text = "Em Atendimento: 0", Margin = new System.Windows.Forms.Padding(0, 0, 8, 0) };
            this.lblResumoAbertos = new System.Windows.Forms.Label { AutoSize = true, Text = "Abertos: 0", Margin = new System.Windows.Forms.Padding(0, 0, 8, 0) };
            this.lblResumoMedia = new System.Windows.Forms.Label { AutoSize = true, Text = "Tempo Médio: 0,0 h", Margin = new System.Windows.Forms.Padding(0, 0, 8, 0) };
            this.lblResumoTaxa = new System.Windows.Forms.Label { AutoSize = true, Text = "Taxa de Resolução: 0,0%", Margin = new System.Windows.Forms.Padding(0, 0, 8, 0) };
            this.lblResumoResolvidosIA = new System.Windows.Forms.Label { AutoSize = true, Text = "Resolvidos via IA: 0", Margin = new System.Windows.Forms.Padding(0, 0, 8, 0), Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold), ForeColor = System.Drawing.Color.FromArgb(33, 150, 83) };

            resumoPanel.Controls.Add(this.lblResumoTotal, 0, 0);
            resumoPanel.Controls.Add(this.lblResumoEncerrados, 1, 0);
            resumoPanel.Controls.Add(this.lblResumoEmAtendimento, 2, 0);
            resumoPanel.Controls.Add(this.lblResumoResolvidosIA, 3, 0);
            resumoPanel.Controls.Add(this.lblResumoAbertos, 0, 1);
            resumoPanel.Controls.Add(this.lblResumoMedia, 1, 1);
            resumoPanel.Controls.Add(this.lblResumoTaxa, 2, 1);


            this.dgv.AllowUserToAddRows = false;
            this.dgv.AllowUserToDeleteRows = false;
            this.dgv.ReadOnly = true;
            this.dgv.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv.MultiSelect = false;
            this.dgv.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv.Margin = new System.Windows.Forms.Padding(12);
            this.dgv.RowHeadersVisible = false;
            this.dgv.BackgroundColor = System.Drawing.Color.White;
            this.dgv.GridColor = System.Drawing.Color.FromArgb(224, 224, 224);
            this.dgv.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.Single;
            this.dgv.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dgv.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dgv.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dgv.EnableHeadersVisualStyles = false;
            this.dgv.ColumnHeadersHeight = 40;
            this.dgv.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            this.dgv.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(60, 60, 60);
            this.dgv.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.dgv.ColumnHeadersDefaultCellStyle.Padding = new System.Windows.Forms.Padding(8, 8, 8, 8);
            this.dgv.ColumnHeadersDefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.dgv.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            this.dgv.DefaultCellStyle.ForeColor = System.Drawing.Color.Black;
            this.dgv.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(230, 237, 255);
            this.dgv.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;
            this.dgv.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dgv.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;

            this.dgv.RowTemplate.Height = 36;
            this.dgv.DefaultCellStyle.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.dgv.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.None;


            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(800, 480);
            this.MinimumSize = new System.Drawing.Size(640, 400);
            this.Padding = new System.Windows.Forms.Padding(12);
            this.Controls.Clear();
            this.Controls.Add(this.dgv);
            this.Controls.Add(resumoPanel);
            this.Controls.Add(topBar);
            this.AcceptButton = this.btnGerar;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "PayHelp - Relatórios";

            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
