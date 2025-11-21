using System.Drawing;
using System.Windows.Forms;

namespace PayHelp.Desktop.WinForms;

public partial class FrmAdminUsuarios : Form
{
    private readonly ApiClient _api;
    private readonly SessionContext _session;
    private DataGridView dgvUsers = null!;
    private Button btnRecarregar = null!;
    private Button btnFechar = null!;

    public FrmAdminUsuarios(ApiClient api, SessionContext session)
    {
        _api = api;
        _session = session;
        InitializeComponent();
        Theme.Apply(this);
        BuildLayout();
    }

    private void BuildLayout()
    {
        this.Text = "Gerenciar Usuários";
        this.Size = new Size(1000, 600);
        this.StartPosition = FormStartPosition.CenterScreen;

        var mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            Padding = new Padding(16)
        };
        mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        // Header
        var header = new Label
        {
            Text = "Gerenciamento de Usuários",
            Font = new Font("Segoe UI", 16f, FontStyle.Bold),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 16)
        };

        // DataGridView
        dgvUsers = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false,
            Margin = new Padding(0, 0, 0, 16)
        };

        dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Visible = false });
        dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nome", HeaderText = "Nome", ReadOnly = true });
        dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "Email", ReadOnly = true });
        dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Role", HeaderText = "Tipo", ReadOnly = true });
        dgvUsers.Columns.Add(new DataGridViewCheckBoxColumn { Name = "IsBlocked", HeaderText = "Bloqueado", ReadOnly = true });

        var btnBlock = new DataGridViewButtonColumn
        {
            Name = "BtnBlock",
            HeaderText = "Bloquear/Desbloquear",
            Text = "Alterar",
            UseColumnTextForButtonValue = true,
            Width = 150
        };
        dgvUsers.Columns.Add(btnBlock);

        var btnResetPwd = new DataGridViewButtonColumn
        {
            Name = "BtnResetPwd",
            HeaderText = "Resetar Senha",
            Text = "Resetar",
            UseColumnTextForButtonValue = true,
            Width = 120
        };
        dgvUsers.Columns.Add(btnResetPwd);

        dgvUsers.CellContentClick += DgvUsers_CellContentClick;

        // Footer buttons
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

        btnRecarregar = new Button
        {
            Text = "Recarregar",
            Tag = "primary",
            AutoSize = true,
            MinimumSize = new Size(100, 36)
        };
        btnRecarregar.Click += async (s, e) => await LoadUsersAsync();

        footer.Controls.Add(btnFechar);
        footer.Controls.Add(btnRecarregar);

        mainPanel.Controls.Add(header, 0, 0);
        mainPanel.Controls.Add(dgvUsers, 0, 1);
        mainPanel.Controls.Add(footer, 0, 2);

        this.Controls.Add(mainPanel);
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
        this.AutoScaleDimensions = new SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(1000, 600);
        this.Name = "FrmAdminUsuarios";
        this.Text = "Gerenciar Usuários";
        this.Load += FrmAdminUsuarios_Load;
        this.ResumeLayout(false);
    }

    private async void FrmAdminUsuarios_Load(object? sender, EventArgs e)
    {
        await LoadUsersAsync();
    }

    private async Task LoadUsersAsync()
    {
        try
        {
            btnRecarregar.Enabled = false;
            dgvUsers.Rows.Clear();

            var users = await _api.ListUsersAsync();
            if (users == null || !users.Any())
            {
                MessageBox.Show("Nenhum usuário encontrado.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            foreach (var u in users)
            {
                dgvUsers.Rows.Add(u.Id, u.Nome, u.Email, u.Role, u.IsBlocked);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao carregar usuários: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnRecarregar.Enabled = true;
        }
    }

    private async void DgvUsers_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;

        var row = dgvUsers.Rows[e.RowIndex];
        var userId = (Guid)row.Cells["Id"].Value;
        var userName = row.Cells["Nome"].Value?.ToString() ?? "";
        var isBlocked = (bool)row.Cells["IsBlocked"].Value;

        if (dgvUsers.Columns[e.ColumnIndex].Name == "BtnBlock")
        {
            await ToggleBlockAsync(userId, userName, isBlocked);
        }
        else if (dgvUsers.Columns[e.ColumnIndex].Name == "BtnResetPwd")
        {
            await ResetPasswordAsync(userId, userName);
        }
    }

    private async Task ToggleBlockAsync(Guid userId, string userName, bool currentlyBlocked)
    {
        var action = currentlyBlocked ? "desbloquear" : "bloquear";
        var confirm = MessageBox.Show(
            $"Deseja {action} o usuário '{userName}'?",
            "Confirmar",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (confirm != DialogResult.Yes) return;

        try
        {
            await _api.BlockUserAsync(userId, !currentlyBlocked);
            MessageBox.Show($"Usuário {(currentlyBlocked ? "desbloqueado" : "bloqueado")} com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadUsersAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao alterar status: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task ResetPasswordAsync(Guid userId, string userName)
    {
        using var dlg = new Form
        {
            Text = "Resetar Senha",
            Size = new Size(400, 180),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var lbl = new Label
        {
            Text = $"Nova senha para '{userName}':",
            Location = new Point(16, 16),
            AutoSize = true
        };

        var txt = new TextBox
        {
            Location = new Point(16, 40),
            Width = 350,
            UseSystemPasswordChar = true
        };

        var btnOk = new Button
        {
            Text = "Resetar",
            DialogResult = DialogResult.OK,
            Location = new Point(266, 80),
            Width = 100
        };

        var btnCancel = new Button
        {
            Text = "Cancelar",
            DialogResult = DialogResult.Cancel,
            Location = new Point(156, 80),
            Width = 100
        };

        dlg.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnCancel });
        dlg.AcceptButton = btnOk;
        dlg.CancelButton = btnCancel;

        Theme.Apply(dlg);

        if (dlg.ShowDialog() == DialogResult.OK)
        {
            var newPassword = txt.Text.Trim();
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                MessageBox.Show("Senha não pode estar vazia.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                await _api.SetUserPasswordAsync(userId, newPassword);
                MessageBox.Show("Senha resetada com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao resetar senha: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
