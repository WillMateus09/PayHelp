using System.Collections.ObjectModel;
using System.Net.Http.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PayHelp.Mobile.Maui.Models;

namespace PayHelp.Mobile.Maui.ViewModels;

public partial class AdminUsersViewModel : ObservableObject
{
    private readonly IHttpClientFactory _factory;

    [ObservableProperty]
    private ObservableCollection<AdminUserDto> _users = new();

    [ObservableProperty]
    private bool _isLoading;

    public AdminUsersViewModel()
    {
        _factory = Helpers.ServiceHelper.GetService<IHttpClientFactory>()!;
    }

    [RelayCommand]
    private async Task LoadUsers()
    {
        try
        {
            IsLoading = true;
            var client = _factory.CreateClient("api");
            var users = await client.GetFromJsonAsync<List<AdminUserDto>>("admin/users");
            if (users != null)
            {
                Users = new ObservableCollection<AdminUserDto>(users);
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Erro", $"Erro ao carregar usuários: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ToggleBlock(AdminUserDto user)
    {
        if (user == null) return;

        try
        {
            var action = user.IsBlocked ? "desbloquear" : "bloquear";
            var confirm = await Application.Current.MainPage.DisplayAlert(
                "Confirmação",
                $"Deseja realmente {action} o usuário {user.Nome}?",
                "Sim", "Não");

            if (!confirm) return;

            var client = _factory.CreateClient("api");
            var payload = new { Blocked = !user.IsBlocked };
            var response = await client.PostAsJsonAsync($"admin/users/{user.Id}/block", payload);
            
            if (response.IsSuccessStatusCode)
            {
                user.IsBlocked = !user.IsBlocked;
                
                await Application.Current.MainPage.DisplayAlert(
                    "Sucesso",
                    $"Usuário {(user.IsBlocked ? "bloqueado" : "desbloqueado")} com sucesso!",
                    "OK");

                await LoadUsersCommand.ExecuteAsync(null);
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Erro", $"Erro ao alterar status: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task ResetPassword(AdminUserDto user)
    {
        if (user == null) return;

        try
        {
            var newPassword = await Application.Current.MainPage.DisplayPromptAsync(
                "Redefinir Senha",
                $"Digite a nova senha para {user.Nome}:",
                "Confirmar",
                "Cancelar",
                placeholder: "Nova senha",
                maxLength: 100);

            if (string.IsNullOrWhiteSpace(newPassword)) return;

            var client = _factory.CreateClient("api");
            var payload = new { NewPassword = newPassword };
            var response = await client.PostAsJsonAsync($"admin/users/{user.Id}/password", payload);
            
            if (response.IsSuccessStatusCode)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Sucesso",
                    $"Senha de {user.Nome} redefinida com sucesso!",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Erro", $"Erro ao redefinir senha: {ex.Message}", "OK");
        }
    }
}
