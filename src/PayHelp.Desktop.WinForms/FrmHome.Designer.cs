namespace PayHelp.Desktop.WinForms;

partial class FrmHome
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.lblUser = new System.Windows.Forms.Label();
        this.btnPainelUsuario = new System.Windows.Forms.Button();
        this.btnPainelSuporte = new System.Windows.Forms.Button();
        this.btnChat = new System.Windows.Forms.Button();
        this.btnMensagens = new System.Windows.Forms.Button();
        this.btnRelatorios = new System.Windows.Forms.Button();
        this.btnSair = new System.Windows.Forms.Button();
        this.SuspendLayout();

        this.lblUser.AutoSize = true;
        this.lblUser.Location = new System.Drawing.Point(12, 9);
        this.lblUser.Name = "lblUser";
        this.lblUser.Size = new System.Drawing.Size(96, 15);
        this.lblUser.Text = "Usuário: (vazio)";

        this.btnPainelUsuario.Location = new System.Drawing.Point(12, 40);
        this.btnPainelUsuario.Name = "btnPainelUsuario";
        this.btnPainelUsuario.Size = new System.Drawing.Size(160, 30);
    this.btnPainelUsuario.Text = "Painel do Usuário";
    this.btnPainelUsuario.Tag = "primary";
        this.btnPainelUsuario.UseVisualStyleBackColor = true;
    this.btnPainelUsuario.Click += new System.EventHandler(this.btnPainelUsuario_Click);

        this.btnPainelSuporte.Location = new System.Drawing.Point(12, 80);
        this.btnPainelSuporte.Name = "btnPainelSuporte";
        this.btnPainelSuporte.Size = new System.Drawing.Size(160, 30);
    this.btnPainelSuporte.Text = "Painel do Suporte";
    this.btnPainelSuporte.Tag = "secondary";
        this.btnPainelSuporte.UseVisualStyleBackColor = true;
    this.btnPainelSuporte.Click += new System.EventHandler(this.btnPainelSuporte_Click);

        this.btnChat.Location = new System.Drawing.Point(12, 120);
        this.btnChat.Name = "btnChat";
        this.btnChat.Size = new System.Drawing.Size(160, 30);
    this.btnChat.Text = "Chat de Chamados";
    this.btnChat.Tag = "secondary";
        this.btnChat.UseVisualStyleBackColor = true;
    this.btnChat.Click += new System.EventHandler(this.btnChat_Click);

        this.btnMensagens.Location = new System.Drawing.Point(12, 160);
        this.btnMensagens.Name = "btnMensagens";
        this.btnMensagens.Size = new System.Drawing.Size(160, 30);
    this.btnMensagens.Text = "Mensagens Automáticas";
    this.btnMensagens.Tag = "secondary";
        this.btnMensagens.UseVisualStyleBackColor = true;
    this.btnMensagens.Click += new System.EventHandler(this.btnMensagens_Click);

        this.btnRelatorios.Location = new System.Drawing.Point(12, 200);
        this.btnRelatorios.Name = "btnRelatorios";
        this.btnRelatorios.Size = new System.Drawing.Size(160, 30);
    this.btnRelatorios.Text = "Relatórios";
    this.btnRelatorios.Tag = "secondary";
        this.btnRelatorios.UseVisualStyleBackColor = true;
    this.btnRelatorios.Click += new System.EventHandler(this.btnRelatorios_Click);

        this.btnSair.Location = new System.Drawing.Point(12, 240);
        this.btnSair.Name = "btnSair";
        this.btnSair.Size = new System.Drawing.Size(160, 30);
    this.btnSair.Text = "Sair";
    this.btnSair.Tag = "danger";
        this.btnSair.UseVisualStyleBackColor = true;
        this.btnSair.Click += new System.EventHandler(this.btnSair_Click);

        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
    this.ClientSize = new System.Drawing.Size(1100, 680);
        this.Controls.Add(this.btnSair);
        this.Controls.Add(this.btnRelatorios);
        this.Controls.Add(this.btnMensagens);
        this.Controls.Add(this.btnChat);
        this.Controls.Add(this.btnPainelSuporte);
        this.Controls.Add(this.btnPainelUsuario);
        this.Controls.Add(this.lblUser);
    this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
    this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Name = "FrmHome";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "PayHelp - Home";
        this.Load += new System.EventHandler(this.FrmHome_Load);
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    private System.Windows.Forms.Label lblUser;
    private System.Windows.Forms.Button btnPainelUsuario;
    private System.Windows.Forms.Button btnPainelSuporte;
    private System.Windows.Forms.Button btnChat;
    private System.Windows.Forms.Button btnMensagens;
    private System.Windows.Forms.Button btnRelatorios;
    private System.Windows.Forms.Button btnSair;
}
