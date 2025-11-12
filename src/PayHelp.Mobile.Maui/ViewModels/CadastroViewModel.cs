using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PayHelp.Mobile.Maui.Models;
using PayHelp.Mobile.Maui.Services;

namespace PayHelp.Mobile.Maui.ViewModels;

public partial class CadastroViewModel : BaseViewModel
{
    private readonly AuthService _auth;

    [ObservableProperty] private string numeroInscricao = string.Empty;
    [ObservableProperty] private string nome = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string senha = string.Empty;
    [ObservableProperty] private bool isSuporte = false;
    [ObservableProperty] private string? palavraVerificacao;

    public CadastroViewModel(AuthService auth)
    {
        _auth = auth;
    }

    [RelayCommand]
    private async Task CadastrarAsync()
    {
        if (IsBusy) return; IsBusy = true;
        try
        {
            if (string.IsNullOrWhiteSpace(NumeroInscricao) || string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha))
            {
                await AlertAsync("Campos obrigatórios", "Preencha Número de Inscrição, Nome, Email e Senha.");
                return;
            }
            if (IsSuporte && string.IsNullOrWhiteSpace(PalavraVerificacao))
            {
                await AlertAsync("Campos obrigatórios", "Informe a Palavra de Verificação para contas de suporte.");
                return;
            }
            var req = new RegisterRequest { NumeroInscricao = NumeroInscricao, Nome = Nome, Email = Email, Senha = Senha, PalavraVerificacao = PalavraVerificacao };
            var result = IsSuporte
                ? await _auth.RegisterSuporteAsync(req)
                : await _auth.RegisterSimplesAsync(req);
            if (!result.ok)
            {
                await AlertAsync("Erro", string.IsNullOrWhiteSpace(result.error) ? "Não foi possível realizar o cadastro." : result.error!);
                return;
            }
            await AlertAsync("Sucesso", "Cadastro realizado! Faça login para continuar.");
            await Application.Current!.MainPage.Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await AlertAsync("Erro", ex.Message);
        }
        finally { IsBusy = false; }
    }
}
