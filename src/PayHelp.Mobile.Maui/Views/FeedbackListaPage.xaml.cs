using PayHelp.Mobile.Maui.ViewModels;

namespace PayHelp.Mobile.Maui.Views;

public partial class FeedbackListaPage : ContentPage
{
    public FeedbackListaPage()
    {
        InitializeComponent();
        BindingContext = Helpers.ServiceHelper.GetService<FeedbackListaViewModel>()!;
    }

    private async void ContentPage_Appearing(object? sender, EventArgs e)
    {
        if (BindingContext is FeedbackListaViewModel vm)
        {
            await vm.CarregarCommand.ExecuteAsync(null);
        }
    }
}
