namespace PayHelp.Desktop.WinForms
{
    partial class FrmCadastro
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.FlowLayoutPanel flpButtons;

        private System.Windows.Forms.Label lblNumero, lblNome, lblEmail, lblSenha, lblConfirmar, lblPalavra;
        private System.Windows.Forms.TextBox txtNumero, txtNome, txtEmail, txtSenha, txtConfirmarSenha, txtPalavra;
        private System.Windows.Forms.Button btnRegistrarSimples, btnRegistrarSuporte;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            tlpMain = new System.Windows.Forms.TableLayoutPanel();
            flpButtons = new System.Windows.Forms.FlowLayoutPanel();

            lblNumero = new System.Windows.Forms.Label();
            lblNome = new System.Windows.Forms.Label();
            lblEmail = new System.Windows.Forms.Label();
            lblSenha = new System.Windows.Forms.Label();
            lblConfirmar = new System.Windows.Forms.Label();
            lblPalavra = new System.Windows.Forms.Label();

            txtNumero = new System.Windows.Forms.TextBox();
            txtNome = new System.Windows.Forms.TextBox();
            txtEmail = new System.Windows.Forms.TextBox();
            txtSenha = new System.Windows.Forms.TextBox();
            txtConfirmarSenha = new System.Windows.Forms.TextBox();
            txtPalavra = new System.Windows.Forms.TextBox();

            btnRegistrarSimples = new System.Windows.Forms.Button();
            btnRegistrarSuporte = new System.Windows.Forms.Button();

            SuspendLayout();


            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(520, 400);
            this.MinimumSize = new System.Drawing.Size(520, 420);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmCadastro";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PayHelp - Cadastro";


            tlpMain.ColumnCount = 2;
            tlpMain.RowCount = 7;
            tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpMain.Padding = new System.Windows.Forms.Padding(16);

            tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 170F));
            tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));


            for (int i = 0; i < 6; i++)
                tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 64F));


            var labelMargin = new System.Windows.Forms.Padding(0, 6, 8, 6);
            foreach (var lbl in new[] { lblNumero, lblNome, lblEmail, lblSenha, lblConfirmar, lblPalavra })
            {
                lbl.AutoSize = false;
                lbl.Dock = System.Windows.Forms.DockStyle.Fill;
                lbl.Margin = labelMargin;
                lbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            }
            lblNumero.Text = "Número";
            lblNome.Text = "Nome";
            lblEmail.Text = "E-mail";
            lblSenha.Text = "Senha";
            lblConfirmar.Text = "Confirmar Senha";
            lblPalavra.Text = "Verificação Segurança";
            lblPalavra.Click += lblPalavra_Click;


            var inputMargin = new System.Windows.Forms.Padding(0, 6, 0, 6);
            foreach (var tb in new[] { txtNumero, txtNome, txtEmail, txtSenha, txtConfirmarSenha })
            {
                tb.Dock = System.Windows.Forms.DockStyle.Fill;
                tb.Margin = inputMargin;
                tb.Size = new System.Drawing.Size(220, 23);
            }
            txtSenha.PasswordChar = '*';
            txtConfirmarSenha.PasswordChar = '*';


            txtPalavra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            txtPalavra.Margin = inputMargin;
            txtPalavra.Size = new System.Drawing.Size(140, 23);


            foreach (var btn in new[] { btnRegistrarSimples, btnRegistrarSuporte })
            {
                btn.AutoSize = true;
                btn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
                btn.MinimumSize = new System.Drawing.Size(120, 36);
                btn.Margin = new System.Windows.Forms.Padding(12, 0, 12, 0);
                btn.Tag = "primary";
            }
            btnRegistrarSimples.Name = "btnRegistrarSimples";
            btnRegistrarSimples.TabIndex = 12;
            btnRegistrarSimples.Text = "Simples";
            btnRegistrarSimples.UseVisualStyleBackColor = true;
            btnRegistrarSimples.Click += btnRegistrarSimples_Click;

            btnRegistrarSuporte.Name = "btnRegistrarSuporte";
            btnRegistrarSuporte.TabIndex = 13;
            btnRegistrarSuporte.Text = "Suporte";
            btnRegistrarSuporte.UseVisualStyleBackColor = true;
            btnRegistrarSuporte.Click += btnRegistrarSuporte_Click;


            flpButtons.AutoSize = true;
            flpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            flpButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            flpButtons.WrapContents = false;
            flpButtons.Margin = new System.Windows.Forms.Padding(0);
            flpButtons.Padding = new System.Windows.Forms.Padding(0);
            flpButtons.Anchor = System.Windows.Forms.AnchorStyles.None;

            flpButtons.Controls.Add(btnRegistrarSimples);
            flpButtons.Controls.Add(btnRegistrarSuporte);


            tlpMain.Controls.Add(lblNumero, 0, 0);
            tlpMain.Controls.Add(txtNumero, 1, 0);

            tlpMain.Controls.Add(lblNome, 0, 1);
            tlpMain.Controls.Add(txtNome, 1, 1);

            tlpMain.Controls.Add(lblEmail, 0, 2);
            tlpMain.Controls.Add(txtEmail, 1, 2);

            tlpMain.Controls.Add(lblSenha, 0, 3);
            tlpMain.Controls.Add(txtSenha, 1, 3);

            tlpMain.Controls.Add(lblConfirmar, 0, 4);
            tlpMain.Controls.Add(txtConfirmarSenha, 1, 4);

            tlpMain.Controls.Add(lblPalavra, 0, 5);
            tlpMain.Controls.Add(txtPalavra, 1, 5);

            tlpMain.SetColumnSpan(flpButtons, 2);
            tlpMain.Controls.Add(flpButtons, 0, 6);


            this.Controls.Clear();
            this.Controls.Add(tlpMain);

            this.ResumeLayout(false);
        }
    }
}
