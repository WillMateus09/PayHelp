namespace PayHelp.Desktop.WinForms;

partial class FrmMensagensAutomaticas
{
    private System.ComponentModel.IContainer components = null;
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.lblTitulo = new System.Windows.Forms.Label();
        this.txtTitulo = new System.Windows.Forms.TextBox();
    this.lblConteudo = new System.Windows.Forms.Label();
    this.txtConteudo = new System.Windows.Forms.TextBox();
    this.lblGatilhos = new System.Windows.Forms.Label();
    this.txtGatilhos = new System.Windows.Forms.TextBox();
    this.btnCriar = new System.Windows.Forms.Button();
    this.btnRemover = new System.Windows.Forms.Button();
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
            ColumnCount = 3
        };
        grid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        grid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        grid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, ButtonColumnWidth));

    grid.RowCount = 3;
    grid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, RowHeight));
    grid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, RowHeight));
    grid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, RowHeight));


        this.lblTitulo.AutoSize = true;
        this.lblTitulo.Text = "Título";
        this.lblTitulo.Margin = new System.Windows.Forms.Padding(0, 8, 8, 0);

        this.txtTitulo.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtTitulo.Margin = new System.Windows.Forms.Padding(0, 4, 12, 0);

        grid.Controls.Add(this.lblTitulo, 0, 0);
        grid.Controls.Add(this.txtTitulo, 1, 0);




        this.lblConteudo.AutoSize = true;
        this.lblConteudo.Text = "Conteúdo";
        this.lblConteudo.Margin = new System.Windows.Forms.Padding(0, 8, 8, 0);

        this.txtConteudo.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtConteudo.Margin = new System.Windows.Forms.Padding(0, 4, 12, 0);


    this.lblGatilhos.AutoSize = true;
    this.lblGatilhos.Text = "Gatilhos (vírgula)";
    this.lblGatilhos.Margin = new System.Windows.Forms.Padding(0, 8, 8, 0);

    this.txtGatilhos.Dock = System.Windows.Forms.DockStyle.Fill;
    this.txtGatilhos.Margin = new System.Windows.Forms.Padding(0, 4, 12, 0);

    this.btnCriar.Text = "Adicionar";
        this.btnCriar.AutoSize = false;
        this.btnCriar.Dock = System.Windows.Forms.DockStyle.Fill;

        this.btnCriar.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
        this.btnCriar.Click += new System.EventHandler(this.btnCriar_Click);

    grid.Controls.Add(this.lblConteudo, 0, 1);
    grid.Controls.Add(this.txtConteudo, 1, 1);

    grid.Controls.Add(this.lblGatilhos, 0, 2);
    grid.Controls.Add(this.txtGatilhos, 1, 2);

    grid.Controls.Add(this.btnCriar, 2, 0);

    grid.Controls.Add(this.btnRemover, 2, 1);


    this.btnRemover.AutoSize = false;
    this.btnRemover.Text = "Excluir";
    this.btnRemover.Dock = System.Windows.Forms.DockStyle.Fill;

    this.btnRemover.Margin = new System.Windows.Forms.Padding(0, 6, 0, 0);
        this.btnRemover.Click += new System.EventHandler(this.btnRemover_Click);

        topBar.Controls.Add(grid);


        this.dgv.AllowUserToAddRows = false;
        this.dgv.AllowUserToDeleteRows = false;
        this.dgv.ReadOnly = true;
        this.dgv.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.dgv.MultiSelect = false;
        this.dgv.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this.dgv.Dock = System.Windows.Forms.DockStyle.Fill;
        this.dgv.Margin = new System.Windows.Forms.Padding(12);


        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.ClientSize = new System.Drawing.Size(720, 480);
        this.Padding = new System.Windows.Forms.Padding(12);
        this.Controls.Clear();
        this.Controls.Add(this.dgv);
        this.Controls.Add(topBar);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
        this.MaximizeBox = true;
        this.MinimizeBox = true;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "PayHelp - Mensagens Automáticas";
        ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.Label lblTitulo;
    private System.Windows.Forms.TextBox txtTitulo;
    private System.Windows.Forms.Label lblConteudo;
    private System.Windows.Forms.TextBox txtConteudo;
    private System.Windows.Forms.Label lblGatilhos;
    private System.Windows.Forms.TextBox txtGatilhos;
    private System.Windows.Forms.Button btnCriar;
    private System.Windows.Forms.Button btnRemover;
    private System.Windows.Forms.DataGridView dgv;
}
