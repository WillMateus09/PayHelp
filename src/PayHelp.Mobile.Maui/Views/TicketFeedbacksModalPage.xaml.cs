using PayHelp.Mobile.Maui.Models;

namespace PayHelp.Mobile.Maui.Views;

public partial class TicketFeedbacksModalPage : ContentPage
{
    public TicketFeedbacksModalPage(IEnumerable<FeedbackCompletoDto> itens)
    {
        InitializeComponent();
        var list = itens?.OrderByDescending(i => i.DataCriacaoUtc).ToList() ?? new();
        List.ItemsSource = list;
        LblCount.Text = $"Total: {list.Count}";
    }

    private async void OnClose(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
