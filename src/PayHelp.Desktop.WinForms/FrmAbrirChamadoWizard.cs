using System;
using System.Windows.Forms;
namespace PayHelp.Desktop.WinForms;

public class FrmAbrirChamadoWizard : Form
{
    private readonly ApiClient _api;
    private readonly SessionContext _session;
    private readonly IServiceProvider _serviceProvider;

    private readonly Label _lblStep = new Label { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(12, 8, 12, 8) };
    private readonly Panel _content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
    private readonly Button _btnVoltar = new Button { Text = "Voltar", Width = 100, Height = 32, Tag = "secondary" };
    private readonly Button _btnAvancar = new Button { Text = "Próximo", Width = 120, Height = 32, Tag = "primary" };
    private readonly Button _btnChamarAtendente = new Button { Text = "Chamar atendente", Width = 160, Height = 32, Tag = "primary" };
    private readonly Button _btnCancelar = new Button { Text = "Cancelar", Width = 100, Height = 32, Tag = "secondary" };

    private int _step = 0;
    private readonly TextBox _txtTitulo = new TextBox { Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
    private readonly TextBox _txtDescricao = new TextBox { Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, Multiline = true, Height = 160 };
    private readonly Label _lblConfirm = new Label { AutoSize = true };
    private Guid? _createdTicketId;
    private bool _creating;

    public FrmAbrirChamadoWizard(ApiClient api, SessionContext session, IServiceProvider serviceProvider)
    {
        _api = api;
        _session = session;
        _serviceProvider = serviceProvider;

        Text = "Abrir Chamado";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimumSize = new System.Drawing.Size(520, 360);
        Size = new System.Drawing.Size(620, 420);

        Theme.Apply(this);

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var footer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, Padding = new Padding(12) };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        _btnCancelar.Click += (_, __) => this.Close();
        _btnVoltar.Click += (_, __) => { if (_step > 0) { _step--; RenderStep(); } }; 
    _btnAvancar.Click += async (_, __) => await NextAsync();
    footer.Controls.Add(_btnVoltar, 0, 0);
    footer.Controls.Add(_btnCancelar, 1, 0);
    var rightPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
    rightPanel.Controls.Add(_btnChamarAtendente);
    rightPanel.Controls.Add(_btnAvancar);
    footer.Controls.Add(rightPanel, 2, 0);

        root.Controls.Add(_lblStep, 0, 0);
        root.Controls.Add(_content, 0, 1);
        root.Controls.Add(footer, 0, 2);
        Controls.Add(root);

        _btnChamarAtendente.Visible = false;
        _btnChamarAtendente.Click += (_, __) => AbrirChatComAtendente();
        RenderStep();
    }

    private void RenderStep()
    {
        _content.Controls.Clear();
        switch (_step)
        {
            case 0:
                _lblStep.Text = "1 de 3 — Título do chamado";
                var t = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 1, RowCount = 2, AutoSize = true };
                t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                t.Controls.Add(new Label { Text = "Informe um título claro", AutoSize = true }, 0, 0);
                t.Controls.Add(_txtTitulo, 0, 1);
                _content.Controls.Add(t);
                _btnVoltar.Enabled = false;
                _btnAvancar.Text = "Próximo";
                break;
            case 1:
                _lblStep.Text = "2 de 3 — Descrição";
                var d = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 1, RowCount = 2, AutoSize = true };
                d.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                d.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                d.Controls.Add(new Label { Text = "Descreva o problema (passos, erros, contexto)", AutoSize = true }, 0, 0);
                d.Controls.Add(_txtDescricao, 0, 1);
                _content.Controls.Add(d);
                _btnVoltar.Enabled = true;
                _btnAvancar.Text = "Próximo";
                break;
            case 2:
                _lblStep.Text = "3 de 3 — Confirmar";
                _lblConfirm.Text = $"Título: {_txtTitulo.Text}\n\nDescrição:\n{_txtDescricao.Text}";
                _content.Controls.Add(_lblConfirm);
                _btnVoltar.Enabled = true;
                _btnAvancar.Text = "Concluir";
                _btnChamarAtendente.Visible = false;
                break;
        }
    }

    private async Task NextAsync()
    {
        if (_creating) return;
        if (_step == 0)
        {
            if (string.IsNullOrWhiteSpace(_txtTitulo.Text))
            {
                MessageBox.Show("Informe um título.");
                return;
            }
            _step++;
            RenderStep();
            return;
        }
        if (_step == 1)
        {

            _step++;
            RenderStep();
            return;
        }


        if (_createdTicketId.HasValue)
        {

            this.Close();
            return;
        }
    _creating = true;
    var oldText = _btnAvancar.Text;
    _btnAvancar.Text = "Criando...";
    _btnAvancar.Enabled = false;
    var oldCursor = this.Cursor; this.Cursor = Cursors.WaitCursor;
        if (_session.CurrentUser is null)
        {
            MessageBox.Show("Você precisa estar logado.");
            _creating = false; _btnAvancar.Enabled = true;
            return;
        }
        var titulo = _txtTitulo.Text.Trim();
        var descricao = _txtDescricao.Text.Trim();
        var created = await _api.AbrirChamadoAsync(_session.CurrentUser.UserId, titulo, descricao);
        if (created is null)
        {
            MessageBox.Show("Não foi possível abrir o chamado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _creating = false; _btnAvancar.Enabled = true;
            return;
        }
        _createdTicketId = created.Id;

        var faq = await _api.BuscarFaqAsync(string.IsNullOrWhiteSpace(descricao) ? titulo : descricao);
        if (faq.Count > 0)
        {
            var top = faq[0];
            var texto = $"FAQ: {top.Solucao}";
            try { await _api.EnviarMensagemAsync(_createdTicketId.Value, _session.CurrentUser.UserId, texto, automatica: true); } catch { }
        }
        else
        {

            var avisoFaq = "Não encontramos uma solução para seu problema no banco de resoluções antigas (FAQ).";
            try { await _api.EnviarMensagemAsync(_createdTicketId.Value, _session.CurrentUser.UserId, avisoFaq, automatica: true); } catch { }
        }


        try
        {
            var chat = _serviceProvider.GetService(typeof(FrmChatChamado)) as FrmChatChamado;
            if (chat != null)
            {
                chat.SetTicket(_createdTicketId.Value);
                chat.Show();
            }
        }
        catch { }


        _creating = false; _btnAvancar.Enabled = true; _btnAvancar.Text = oldText; this.Cursor = oldCursor;
        this.BeginInvoke(new Action(this.Close));
    }

    private void AbrirChatComAtendente()
    {
        if (_btnChamarAtendente.Tag is Guid ticketId)
        {
            var chat = _serviceProvider.GetService(typeof(FrmChatChamado)) as FrmChatChamado;
            if (chat != null)
            {
                chat.SetTicket(ticketId);
                chat.Show();
            }
        }
    }
}
