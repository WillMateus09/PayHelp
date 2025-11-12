namespace PayHelp.Desktop.WinForms;

partial class FrmLogin
{



    private System.ComponentModel.IContainer components = null;





    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code





    private void InitializeComponent()
    {
        this.lblEmail = new System.Windows.Forms.Label();
        this.lblSenha = new System.Windows.Forms.Label();
        this.txtEmail = new System.Windows.Forms.TextBox();
        this.txtSenha = new System.Windows.Forms.TextBox();
        this.btnLogin = new System.Windows.Forms.Button();
        this.lnkCadastro = new System.Windows.Forms.LinkLabel();
        this.SuspendLayout();



        this.lblEmail.AutoSize = true;
        this.lblEmail.Location = new System.Drawing.Point(24, 22);
        this.lblEmail.Name = "lblEmail";
        this.lblEmail.Size = new System.Drawing.Size(41, 15);
        this.lblEmail.TabIndex = 0;
        this.lblEmail.Text = "E-mail";



        this.lblSenha.AutoSize = true;
        this.lblSenha.Location = new System.Drawing.Point(24, 64);
        this.lblSenha.Name = "lblSenha";
        this.lblSenha.Size = new System.Drawing.Size(39, 15);
        this.lblSenha.TabIndex = 1;
        this.lblSenha.Text = "Senha";



        this.txtEmail.Location = new System.Drawing.Point(88, 19);
        this.txtEmail.Name = "txtEmail";
        this.txtEmail.Size = new System.Drawing.Size(240, 23);
        this.txtEmail.TabIndex = 2;



        this.txtSenha.Location = new System.Drawing.Point(88, 61);
        this.txtSenha.Name = "txtSenha";
        this.txtSenha.PasswordChar = '*';
        this.txtSenha.Size = new System.Drawing.Size(240, 23);
        this.txtSenha.TabIndex = 3;



        this.btnLogin.Location = new System.Drawing.Point(253, 102);
        this.btnLogin.Name = "btnLogin";
        this.btnLogin.Size = new System.Drawing.Size(75, 23);
        this.btnLogin.TabIndex = 4;
    this.btnLogin.Text = "Entrar";
    this.btnLogin.Tag = "primary";
        this.btnLogin.UseVisualStyleBackColor = true;
        this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);



    this.lnkCadastro.AutoSize = true;
    this.lnkCadastro.Location = new System.Drawing.Point(24, 106);
    this.lnkCadastro.Name = "lnkCadastro";
    this.lnkCadastro.Size = new System.Drawing.Size(126, 15);
    this.lnkCadastro.TabIndex = 5;
    this.lnkCadastro.TabStop = true;
    this.lnkCadastro.Text = "Não tem conta? Cadastre-se";
    this.lnkCadastro.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkCadastro_LinkClicked);



        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
    this.ClientSize = new System.Drawing.Size(354, 151);
        this.Controls.Add(this.btnLogin);
        this.Controls.Add(this.txtSenha);
        this.Controls.Add(this.txtEmail);
    this.Controls.Add(this.lnkCadastro);
        this.Controls.Add(this.lblSenha);
        this.Controls.Add(this.lblEmail);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "FrmLogin";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "PayHelp - Login";
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label lblEmail;
    private System.Windows.Forms.Label lblSenha;
    private System.Windows.Forms.TextBox txtEmail;
    private System.Windows.Forms.TextBox txtSenha;
    private System.Windows.Forms.Button btnLogin;
    private System.Windows.Forms.LinkLabel lnkCadastro;
}
