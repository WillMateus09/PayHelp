namespace PayHelp.Desktop.WinForms;

partial class FrmChatChamado
{
    private System.ComponentModel.IContainer components = null;
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }
    private void InitializeComponent()
    {
        this.lblTicket = new System.Windows.Forms.Label();
        this.cmbTickets = new System.Windows.Forms.ComboBox();
        this.btnRecarregar = new System.Windows.Forms.Button();
        this.lstMensagens = new System.Windows.Forms.ListBox();
        this.txtMensagem = new System.Windows.Forms.TextBox();
        this.btnEnviar = new System.Windows.Forms.Button();
        this.btnTriagem = new System.Windows.Forms.Button();
        this.lblSugestao = new System.Windows.Forms.Label();
        this.SuspendLayout();

    var topBar = new System.Windows.Forms.Panel { Dock = System.Windows.Forms.DockStyle.Top, Height = 48, Padding = new System.Windows.Forms.Padding(12, 8, 12, 8) };
    var filterRow = new System.Windows.Forms.FlowLayoutPanel { Dock = System.Windows.Forms.DockStyle.Fill, FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight, WrapContents = false, AutoSize = true };
    var lblTriaging = new System.Windows.Forms.Label { AutoSize = true, Padding = new System.Windows.Forms.Padding(8, 4, 8, 4), Visible = false };
    lblTriaging.Name = "lblTriaging";
    lblTriaging.Text = "IA em triagem";
    lblTriaging.BackColor = System.Drawing.Color.LightSkyBlue;
    lblTriaging.ForeColor = System.Drawing.Color.White;
    lblTriaging.Margin = new System.Windows.Forms.Padding(12, 0, 0, 0);
    this.lblTicket.AutoSize = true; this.lblTicket.Text = "Ticket:";
    this.cmbTickets.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList; this.cmbTickets.Width = 360; this.cmbTickets.SelectedIndexChanged += new System.EventHandler(this.cmbTickets_SelectedIndexChanged);
    this.btnRecarregar.AutoSize = true; this.btnRecarregar.Text = "Recarregar"; this.btnRecarregar.Click += new System.EventHandler(this.btnRecarregar_Click);
    filterRow.Controls.Add(this.lblTicket);
    filterRow.Controls.Add(this.cmbTickets);
    filterRow.Controls.Add(this.btnRecarregar);
    filterRow.Controls.Add(lblTriaging);
    topBar.Controls.Add(filterRow);


    this.lstMensagens.Dock = System.Windows.Forms.DockStyle.Fill; this.lstMensagens.Margin = new System.Windows.Forms.Padding(12);


    var bottomBar = new System.Windows.Forms.Panel { Dock = System.Windows.Forms.DockStyle.Bottom, Height = 72, Padding = new System.Windows.Forms.Padding(12, 8, 12, 8) };
    var composeRow = new System.Windows.Forms.TableLayoutPanel { Dock = System.Windows.Forms.DockStyle.Fill, ColumnCount = 3 };
    composeRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
    composeRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
    composeRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
    this.txtMensagem.Dock = System.Windows.Forms.DockStyle.Fill;
    this.btnEnviar.AutoSize = true; this.btnEnviar.Text = "Enviar"; this.btnEnviar.Click += new System.EventHandler(this.btnEnviar_Click);
    this.btnTriagem.AutoSize = true; this.btnTriagem.Text = "Triagem"; this.btnTriagem.Click += new System.EventHandler(this.btnTriagem_Click);
    composeRow.Controls.Add(this.txtMensagem, 0, 0);
    composeRow.Controls.Add(this.btnEnviar, 1, 0);
    composeRow.Controls.Add(this.btnTriagem, 2, 0);
    bottomBar.Controls.Add(composeRow);


    this.lblSugestao.AutoSize = true; this.lblSugestao.Dock = System.Windows.Forms.DockStyle.Bottom; this.lblSugestao.Padding = new System.Windows.Forms.Padding(12, 0, 12, 6); this.lblSugestao.Text = "Sugestão: (vazia)";


    this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
    this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
    this.ClientSize = new System.Drawing.Size(820, 560);
    this.Controls.Clear();
    this.Controls.Add(this.lstMensagens);
    this.Controls.Add(bottomBar);
    this.Controls.Add(this.lblSugestao);
    this.Controls.Add(topBar);
    this.Controls.SetChildIndex(topBar, 0);
    this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable; this.MaximizeBox = true; this.MinimizeBox = true;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "PayHelp - Chat de Chamados";
        this.ResumeLayout(false);

    }

    private System.Windows.Forms.Label lblTicket;
    private System.Windows.Forms.ComboBox cmbTickets;
    private System.Windows.Forms.Button btnRecarregar;
    private System.Windows.Forms.ListBox lstMensagens;
    private System.Windows.Forms.TextBox txtMensagem;
    private System.Windows.Forms.Button btnEnviar;
    private System.Windows.Forms.Button btnTriagem;
    private System.Windows.Forms.Label lblSugestao;
}
