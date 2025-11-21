using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PayHelp.Mobile.Maui.Utilities;

namespace PayHelp.Mobile.Maui.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    [ObservableProperty] private string userName = string.Empty;
    [ObservableProperty] private string userEmail = string.Empty;
    [ObservableProperty] private string userRole = string.Empty;
    [ObservableProperty] private bool isSuporte;

    public string DisplayUser => string.IsNullOrWhiteSpace(UserName)
        ? (string.IsNullOrWhiteSpace(UserEmail) ? "" : UserEmail)
        : string.IsNullOrWhiteSpace(UserRole) ? UserName : $"{UserName} ({UserRole})";

    partial void OnUserNameChanged(string value) => OnPropertyChanged(nameof(DisplayUser));
    partial void OnUserRoleChanged(string value) => OnPropertyChanged(nameof(DisplayUser));
    partial void OnUserEmailChanged(string value) => OnPropertyChanged(nameof(DisplayUser));

    public async Task LoadAsync()
    {
        UserName = (await AppSettings.GetUserNameAsync()) ?? string.Empty;
        UserEmail = (await AppSettings.GetUserEmailAsync()) ?? string.Empty;
        UserRole = (await AppSettings.GetUserRoleAsync()) ?? string.Empty;
        
        // Verificar se é suporte
        var role = UserRole.ToLowerInvariant();
        IsSuporte = role.Contains("suporte") || role.Contains("support");
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        if (IsBusy) return; IsBusy = true;
        try
        {
            await AppSettings.ClearAuthAsync();
            Application.Current!.MainPage = new NavigationPage(new Views.LoginPage());
        }
        finally { IsBusy = false; }
    }
}
