using CommunityToolkit.Mvvm.ComponentModel;

namespace PayHelp.Mobile.Maui.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    bool isBusy;

    public bool IsNotBusy => !IsBusy;

    protected async Task AlertAsync(string title, string message)
    {
        if (Shell.Current is not null)
            await Shell.Current.DisplayAlert(title, message, "OK");
    }

    protected async Task<string?> PromptAsync(string title, string message, string placeholder = "", string accept = "OK", string cancel = "Cancelar")
    {
        if (Shell.Current is null) return null;
        return await Shell.Current.DisplayPromptAsync(title, message, accept, cancel, placeholder);
    }
}
