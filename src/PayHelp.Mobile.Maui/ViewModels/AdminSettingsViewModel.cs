using System.Net.Http.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PayHelp.Mobile.Maui.ViewModels;

public partial class AdminSettingsViewModel : ObservableObject
{
    private readonly IHttpClientFactory _factory;

    [ObservableProperty]
    private string _supportVerificationWord = string.Empty;

    [ObservableProperty]
    private string _publicBaseUrl = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public AdminSettingsViewModel()
    {
        _factory = Helpers.ServiceHelper.GetService<IHttpClientFactory>()!;
    }

    [RelayCommand]
    private async Task LoadSettings()
    {
        try
        {
            IsLoading = true;
            StatusMessage = string.Empty;

            var client = _factory.CreateClient("api");
            var settings = await client.GetFromJsonAsync<AdminSettingsDto>("admin/settings");
            
            if (settings != null)
            {
                SupportVerificationWord = settings.SupportVerificationWord ?? string.Empty;
                PublicBaseUrl = settings.PublicBaseUrl ?? string.Empty;
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Erro", $"Erro ao carregar configurações: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveSettings()
    {
        try
        {
            IsLoading = true;
            StatusMessage = string.Empty;

            var payload = new
            {
                SupportVerificationWord = SupportVerificationWord,
                PublicBaseUrl = PublicBaseUrl
            };

            var client = _factory.CreateClient("api");
            var response = await client.PutAsJsonAsync("admin/settings", payload);
            
            if (response.IsSuccessStatusCode)
            {
                StatusMessage = "✅ Configurações salvas com sucesso!";
                
                await Application.Current.MainPage.DisplayAlert(
                    "Sucesso",
                    "Configurações salvas com sucesso!",
                    "OK");

                // Clear status message after 3 seconds
                await Task.Delay(3000);
                StatusMessage = string.Empty;
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Erro", $"Erro ao salvar configurações: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public class AdminSettingsDto
    {
        public string? SupportVerificationWord { get; set; }
        public string? PublicBaseUrl { get; set; }
    }
}
