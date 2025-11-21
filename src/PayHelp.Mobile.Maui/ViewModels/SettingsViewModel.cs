using System.Net.Http;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PayHelp.Mobile.Maui.Utilities;

namespace PayHelp.Mobile.Maui.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private string baseUrl = AppSettings.BaseApiUrl;

    [ObservableProperty]
    private string? testMessage;

    [ObservableProperty]
    private bool testOk;

    public IRelayCommand SaveCommand { get; }
    public IRelayCommand TestCommand { get; }
    public IRelayCommand UseAndroidEmulatorCommand { get; }

    public SettingsViewModel()
    {
        SaveCommand = new RelayCommand(Save);
        TestCommand = new RelayCommand(async () => await TestAsync());
        UseAndroidEmulatorCommand = new RelayCommand(UseAndroidEmulator);
    }

    private void Save()
    {
        var url = (BaseUrl ?? string.Empty).Trim();
        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
        {
            TestOk = false;
            TestMessage = "Informe uma URL iniciando com http:// ou https://";
            return;
        }
        // Normaliza e persiste
        if (!url.EndsWith("/")) url += "/";
        AppSettings.SetBaseApiUrl(url);
        BaseUrl = AppSettings.BaseApiUrl; // readback normalized
        TestOk = true;
        TestMessage = "URL salva. As próximas chamadas usam esse endereço.";
    }

    private async Task TestAsync()
    {
        try
        {
            var url = (BaseUrl ?? string.Empty).TrimEnd('/');
            if (string.IsNullOrWhiteSpace(url)) { TestOk = false; TestMessage = "Informe a URL da API"; return; }
            // Se terminar com /api, remove para acessar /__version na raiz
            var root = url.EndsWith("/api", StringComparison.OrdinalIgnoreCase) ? url[..^4] : url;
            var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var resp = await http.GetAsync($"{root}/__version");
            if (!resp.IsSuccessStatusCode)
            {
                TestOk = false;
                TestMessage = $"Falha: {(int)resp.StatusCode} {resp.ReasonPhrase}";
                return;
            }
            TestOk = true;
            TestMessage = "Conexão OK (/_version)";
        }
        catch (TaskCanceledException)
        {
            TestOk = false;
            TestMessage = "Timeout ao conectar. Verifique IP/porta e firewall.";
        }
        catch (Exception ex)
        {
            TestOk = false;
            TestMessage = $"Erro: {ex.Message}";
        }
    }

    private void UseAndroidEmulator()
    {
        BaseUrl = "http://10.0.2.2:5236/api/";
    }
}
