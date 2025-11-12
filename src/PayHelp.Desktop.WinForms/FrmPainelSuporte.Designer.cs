namespace PayHelp.Desktop.WinForms
{
    partial class FrmPainelSuporte
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblFiltro = new System.Windows.Forms.Label();
            cmbStatus = new System.Windows.Forms.ComboBox();
            btnRecarregar = new System.Windows.Forms.Button();
            dgvTickets = new System.Windows.Forms.DataGridView();
            btnAssumir = new System.Windows.Forms.Button();
            btnEmAtendimento = new System.Windows.Forms.Button();
            btnEncerrar = new System.Windows.Forms.Button();
            topBar = new System.Windows.Forms.Panel();
            filterRow = new System.Windows.Forms.FlowLayoutPanel();
            bottomBar = new System.Windows.Forms.Panel();
            actions = new System.Windows.Forms.FlowLayoutPanel();
            contentPanel = new System.Windows.Forms.Panel();

            ((System.ComponentModel.ISupportInitialize)(dgvTickets)).BeginInit();
            topBar.SuspendLayout();
            filterRow.SuspendLayout();
            bottomBar.SuspendLayout();
            actions.SuspendLayout();
            contentPanel.SuspendLayout();
            SuspendLayout();


            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(900, 609);
            Name = "FrmPainelSuporte";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "PayHelp - Painel Suporte";

            Padding = new System.Windows.Forms.Padding(8);



            topBar.AutoSize = false;
            topBar.Height = 56;
            topBar.Dock = System.Windows.Forms.DockStyle.Top;
            topBar.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            topBar.Name = "topBar";
            topBar.TabIndex = 0;


            filterRow.AutoSize = true;
            filterRow.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;

            filterRow.Dock = System.Windows.Forms.DockStyle.Left;
            filterRow.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            filterRow.WrapContents = false;
            filterRow.Name = "filterRow";
            filterRow.TabIndex = 0;


            lblFiltro.AutoSize = true;
            lblFiltro.Margin = new System.Windows.Forms.Padding(0, 6, 8, 6);
            lblFiltro.Name = "lblFiltro";
            lblFiltro.Text = "Status:";
            lblFiltro.TabIndex = 0;


            cmbStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbStatus.Margin = new System.Windows.Forms.Padding(0, 3, 12, 3);
            cmbStatus.Name = "cmbStatus";
            cmbStatus.Size = new System.Drawing.Size(180, 23);
            cmbStatus.TabIndex = 1;
            cmbStatus.SelectedIndexChanged += cmbStatus_SelectedIndexChanged;


            btnRecarregar.AutoSize = true;
            btnRecarregar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnRecarregar.MinimumSize = new System.Drawing.Size(110, 32);
            btnRecarregar.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            btnRecarregar.Name = "btnRecarregar";
            btnRecarregar.TabIndex = 2;
            btnRecarregar.Text = "Recarregar";
            btnRecarregar.Tag = "secondary";
            btnRecarregar.UseVisualStyleBackColor = true;
            btnRecarregar.Click += btnRecarregar_Click;


            bottomBar.AutoSize = false;
            bottomBar.Height = 64;
            bottomBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            bottomBar.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            bottomBar.Name = "bottomBar";
            bottomBar.TabIndex = 2;


            actions.AutoSize = true;
            actions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;

            actions.Dock = System.Windows.Forms.DockStyle.Right;
            actions.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            actions.WrapContents = false;
            actions.Name = "actions";
            actions.TabIndex = 0;


            btnAssumir.AutoSize = true;
            btnAssumir.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnAssumir.MinimumSize = new System.Drawing.Size(110, 36);
            btnAssumir.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            btnAssumir.Name = "btnAssumir";
            btnAssumir.TabIndex = 0;
            btnAssumir.Text = "Assumir";
            btnAssumir.Tag = "primary";
            btnAssumir.UseVisualStyleBackColor = true;
            btnAssumir.Click += btnAssumir_Click;


            btnEmAtendimento.AutoSize = true;
            btnEmAtendimento.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnEmAtendimento.MinimumSize = new System.Drawing.Size(144, 36);
            btnEmAtendimento.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            btnEmAtendimento.Name = "btnEmAtendimento";
            btnEmAtendimento.TabIndex = 1;
            btnEmAtendimento.Text = "Marcar EmAtendimento";
            btnEmAtendimento.Tag = "secondary";
            btnEmAtendimento.UseVisualStyleBackColor = true;
            btnEmAtendimento.Click += btnEmAtendimento_Click;


            btnEncerrar.AutoSize = true;
            btnEncerrar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnEncerrar.MinimumSize = new System.Drawing.Size(110, 36);
            btnEncerrar.Margin = new System.Windows.Forms.Padding(0);
            btnEncerrar.Name = "btnEncerrar";
            btnEncerrar.TabIndex = 2;
            btnEncerrar.Text = "Encerrar";
            btnEncerrar.Tag = "danger";
            btnEncerrar.UseVisualStyleBackColor = true;
            btnEncerrar.Click += btnEncerrar_Click;



            contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            contentPanel.Padding = new System.Windows.Forms.Padding(12);
            contentPanel.Name = "contentPanel";
            contentPanel.TabIndex = 3;


            dgvTickets.AllowUserToAddRows = false;
            dgvTickets.AllowUserToDeleteRows = false;
            dgvTickets.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;

            dgvTickets.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvTickets.Margin = new System.Windows.Forms.Padding(0);
            dgvTickets.MultiSelect = false;
            dgvTickets.Name = "dgvTickets";
            dgvTickets.ReadOnly = true;
            dgvTickets.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvTickets.TabIndex = 1;



            filterRow.Controls.Add(lblFiltro);
            filterRow.Controls.Add(cmbStatus);
            filterRow.Controls.Add(btnRecarregar);
            topBar.Controls.Add(filterRow);


            actions.Controls.Add(btnAssumir);
            btnEmAtendimento.Visible = false;
            btnEncerrar.Visible = false;
            bottomBar.Controls.Add(actions);


            contentPanel.Controls.Add(dgvTickets);


            Controls.Add(contentPanel);
            Controls.Add(bottomBar);
            Controls.Add(topBar);

            ((System.ComponentModel.ISupportInitialize)(dgvTickets)).EndInit();
            topBar.ResumeLayout(false);
            topBar.PerformLayout();
            filterRow.ResumeLayout(false);
            filterRow.PerformLayout();
            bottomBar.ResumeLayout(false);
            bottomBar.PerformLayout();
            actions.ResumeLayout(false);
            actions.PerformLayout();
            contentPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        private System.Windows.Forms.Label lblFiltro;
        private System.Windows.Forms.ComboBox cmbStatus;
        private System.Windows.Forms.Button btnRecarregar;
        private System.Windows.Forms.DataGridView dgvTickets;
        private System.Windows.Forms.Button btnAssumir;
        private System.Windows.Forms.Button btnEmAtendimento;
        private System.Windows.Forms.Button btnEncerrar;
        private System.Windows.Forms.Panel topBar;
        private System.Windows.Forms.FlowLayoutPanel filterRow;
        private System.Windows.Forms.Panel bottomBar;
        private System.Windows.Forms.FlowLayoutPanel actions;
        private System.Windows.Forms.Panel contentPanel;
    }
}
