using PayHelp.Mobile.Maui.Models;

namespace PayHelp.Mobile.Maui.Views;

public partial class FeedbackModalPage : ContentPage
{
    public FeedbackModalPage(FeedbackDto feedback)
    {
        InitializeComponent();
        CarregarFeedback(feedback);
    }

    private void CarregarFeedback(FeedbackDto feedback)
    {
        // Atualizar estrelas
        var stars = new[] { Star1, Star2, Star3, Star4, Star5 };
        var nota = feedback.NotaUsuario ?? 0;
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].TextColor = i < nota ? Color.FromArgb("#FFC107") : Color.FromArgb("#E0E0E0");
        }

        // Atualizar nota
        LblNota.Text = $"{nota}/5";

        // Atualizar data
        var dataLocal = feedback.DataResolvidoUsuario?.ToLocalTime() ?? DateTime.Now;
        LblData.Text = $"Resolvido em: {dataLocal:dd/MM/yyyy HH:mm}";

        // Atualizar comentário
        LblComentario.Text = string.IsNullOrWhiteSpace(feedback.FeedbackUsuario) 
            ? "(Sem comentário)" 
            : feedback.FeedbackUsuario;
    }

    private async void OnFecharClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
