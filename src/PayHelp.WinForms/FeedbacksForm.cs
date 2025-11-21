using PayHelp.Client;
using PayHelp.Client.Dtos;

namespace PayHelp.WinForms;

public class FeedbacksForm : Form
{
    private readonly ApiService _api;
    private readonly ITokenStore _tokenStore;
    private readonly Guid _ticketId;
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
    private readonly Label _status = new() { Dock = DockStyle.Bottom, Height = 24, ForeColor = Color.Red };

    public FeedbacksForm(ApiService api, ITokenStore tokenStore, Guid ticketId)
    {
        _api = api; _tokenStore = tokenStore; _ticketId = ticketId;
        Text = "Feedbacks do Ticket";
        Width = 700; Height = 400;
        Controls.Add(_grid);
        Controls.Add(_status);
        Load += async (_, __) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            _status.Text = string.Empty;
            var list = (await _api.GetFeedbacksDoTicketAsync(_ticketId)).ToList();
            _grid.DataSource = list;
        }
        catch (ApiUnauthorizedException)
        {
            await _tokenStore.ClearAsync();
            MessageBox.Show("Sessão expirada. Faça login novamente.");
            Close();
        }
        catch (Exception ex)
        {
            _status.Text = ex.Message;
        }
    }
}
