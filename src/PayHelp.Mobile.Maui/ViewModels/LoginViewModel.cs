using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PayHelp.Mobile.Maui.Models;
using PayHelp.Mobile.Maui.Services;
using PayHelp.Mobile.Maui.Utilities;

namespace PayHelp.Mobile.Maui.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly AuthService _auth;

    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string senha = string.Empty;

    [ObservableProperty] private string? loginError = string.Empty;

    [ObservableProperty] private bool senhaOculta = true;


    public bool HasLoginError => !string.IsNullOrWhiteSpace(LoginError);


    partial void OnLoginErrorChanged(string? value) => OnPropertyChanged(nameof(HasLoginError));

    public LoginViewModel(AuthService auth)
    {
        _auth = auth;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy) return; IsBusy = true;
        try
        {
            LoginError = string.Empty;
            var (res, err) = await _auth.LoginAsync(new LoginRequest { Email = Email, Senha = Senha });
            if (res == null || string.IsNullOrWhiteSpace(res.EffectiveToken))
            {
                var mensagem = string.IsNullOrWhiteSpace(err) ? "Verifique suas credenciais." : err!;

                LoginError = mensagem;

                await AlertAsync("Falha no login", mensagem);
                return;
            }

            await AppSettings.SaveAuthAsync(res.EffectiveToken, res.UserId, res.Nome ?? string.Empty, res.Email ?? Email, res.Role ?? string.Empty);

            var role = (res.Role ?? string.Empty).ToLowerInvariant();
            var isSuporte = role.Contains("suporte") || role.Contains("support");
            var isMaster = role.Contains("master");

            // Define o Shell baseado na role
            if (isMaster)
            {
                Application.Current!.MainPage = new MasterShell();
                await Shell.Current.GoToAsync("//home");
            }
            else if (isSuporte)
            {
                Application.Current!.MainPage = new AppShell();
                await Shell.Current.GoToAsync("//home");
            }
            else
            {
                Application.Current!.MainPage = new UserShell();
                await Shell.Current.GoToAsync("//home");
            }
        }
        catch (Exception ex)
        {
            var mensagem = $"Não foi possível realizar o login: {ex.Message}";
            LoginError = mensagem;
            await AlertAsync("Erro", mensagem);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task NavigateCadastroAsync()
    {
        await Application.Current!.MainPage.Navigation.PushAsync(new Views.CadastroPage());
    }

    [RelayCommand]
    private void AlternarSenhaOculta()
    {
        SenhaOculta = !SenhaOculta;
    }
}
