using System.Drawing;
using System.Windows.Forms;

namespace PayHelp.Desktop.WinForms;

public partial class FrmAdminConfiguracoes : Form
{
    private readonly ApiClient _api;
    private TextBox txtSupportWord = null!;
    private TextBox txtPublicUrl = null!;
    private Button btnSalvar = null!;
    private Button btnFechar = null!;

    public FrmAdminConfiguracoes(ApiClient api)
    {
        _api = api;
        InitializeComponent();
        Theme.Apply(this);
        BuildLayout();
    }

    private void BuildLayout()
    {
        this.Text = "Configurações do Sistema";
        this.Size = new Size(600, 300);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;

        var mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 4,
            ColumnCount = 2,
            Padding = new Padding(16)
        };
        mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        // Support Verification Word
        var lblSupportWord = new Label
        {
            Text = "Palavra de Verificação:",
            TextAlign = ContentAlignment.MiddleRight,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 8, 8, 8)
        };

        txtSupportWord = new TextBox
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 8, 0, 8),
            PlaceholderText = "Ex: payhelp2024"
        };

        // Public Base URL
        var lblPublicUrl = new Label
        {
            Text = "URL Base Pública:",
            TextAlign = ContentAlignment.MiddleRight,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 8, 8, 8)
        };

        txtPublicUrl = new TextBox
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 8, 0, 8),
            PlaceholderText = "Ex: http://192.168.1.10:5236/ ou https://api.payhelp.com/"
        };

        // Footer
        var footer = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true
        };

        btnFechar = new Button
        {
            Text = "Fechar",
            Tag = "outline",
            AutoSize = true,
            MinimumSize = new Size(100, 36),
            Margin = new Padding(0, 0, 8, 0)
        };
        btnFechar.Click += (s, e) => Close();

        btnSalvar = new Button
        {
            Text = "Salvar",
            Tag = "primary",
            AutoSize = true,
            MinimumSize = new Size(100, 36)
        };
        btnSalvar.Click += BtnSalvar_Click;

        footer.Controls.Add(btnFechar);
        footer.Controls.Add(btnSalvar);

        mainPanel.Controls.Add(lblSupportWord, 0, 0);
        mainPanel.Controls.Add(txtSupportWord, 1, 0);
        mainPanel.Controls.Add(lblPublicUrl, 0, 1);
        mainPanel.Controls.Add(txtPublicUrl, 1, 1);

        mainPanel.SetColumnSpan(footer, 2);
        mainPanel.Controls.Add(footer, 0, 3);

        this.Controls.Add(mainPanel);
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
        this.AutoScaleDimensions = new SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(600, 300);
        this.Name = "FrmAdminConfiguracoes";
        this.Text = "Configurações do Sistema";
        this.Load += FrmAdminConfiguracoes_Load;
        this.ResumeLayout(false);
    }

    private async void FrmAdminConfiguracoes_Load(object? sender, EventArgs e)
    {
        await LoadSettingsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            btnSalvar.Enabled = false;
            var settings = await _api.GetAdminSettingsAsync();
            if (settings != null)
            {
                txtSupportWord.Text = settings.SupportVerificationWord ?? "";
                txtPublicUrl.Text = settings.PublicBaseUrl ?? "";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao carregar configurações: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnSalvar.Enabled = true;
        }
    }

    private async void BtnSalvar_Click(object? sender, EventArgs e)
    {
        try
        {
            btnSalvar.Enabled = false;

            var supportWord = txtSupportWord.Text.Trim();
            var publicUrl = txtPublicUrl.Text.Trim();

            if (string.IsNullOrWhiteSpace(supportWord))
            {
                MessageBox.Show("Palavra de verificação não pode estar vazia.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await _api.UpdateAdminSettingsAsync(supportWord, publicUrl);
            MessageBox.Show("Configurações salvas com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao salvar configurações: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnSalvar.Enabled = true;
        }
    }
}
