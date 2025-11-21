using Microsoft.Maui.Storage;

namespace PayHelp.Mobile.Maui.Utilities;

public static class AppSettings
{

    public static string BaseApiUrl { get; set; } = "http://192.168.15.107:5236/api/";

    public const string TokenKey = "auth_token";
    public const string UserIdKey = "user_id";
    public const string UserNameKey = "user_name";
    public const string UserEmailKey = "user_email";
    public const string UserRoleKey = "user_role";

    public static async Task SaveAuthAsync(string token, Guid userId, string nome, string email, string role)
    {
        await SecureStorage.Default.SetAsync(TokenKey, token ?? string.Empty);
        await SecureStorage.Default.SetAsync(UserIdKey, userId.ToString());
        await SecureStorage.Default.SetAsync(UserNameKey, nome ?? string.Empty);
        await SecureStorage.Default.SetAsync(UserEmailKey, email ?? string.Empty);
        await SecureStorage.Default.SetAsync(UserRoleKey, role ?? string.Empty);
    }

    public static async Task ClearAuthAsync()
    {
        SecureStorage.Default.Remove(TokenKey);
        SecureStorage.Default.Remove(UserIdKey);
        SecureStorage.Default.Remove(UserNameKey);
        SecureStorage.Default.Remove(UserEmailKey);
        SecureStorage.Default.Remove(UserRoleKey);
        await Task.CompletedTask;
    }

    public static async Task<string?> GetTokenAsync() => await SecureStorage.Default.GetAsync(TokenKey);

    public static Task<string?> GetUserNameAsync() => SecureStorage.Default.GetAsync(UserNameKey);
    public static Task<string?> GetUserEmailAsync() => SecureStorage.Default.GetAsync(UserEmailKey);
    public static Task<string?> GetUserRoleAsync() => SecureStorage.Default.GetAsync(UserRoleKey);
    
    public static void SetBaseApiUrl(string url)
    {
        if (!string.IsNullOrWhiteSpace(url))
            BaseApiUrl = url.TrimEnd('/') + "/";
    }
}
