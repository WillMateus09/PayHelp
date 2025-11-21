using System.Security.Cryptography;
using PayHelp.Client;

namespace PayHelp.WinForms;

public sealed class WinFormsTokenStore : ITokenStore
{
    private static string FilePath
    {
        get
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PayHelp");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "token.bin");
        }
    }

    public Task ClearAsync()
    {
        if (File.Exists(FilePath))
            File.Delete(FilePath);
        return Task.CompletedTask;
    }

    public Task<string?> GetAsync()
    {
        if (!File.Exists(FilePath))
            return Task.FromResult<string?>(null);

        try
        {
            var protectedBytes = File.ReadAllBytes(FilePath);
            var unprotected = ProtectedData.Unprotect(protectedBytes, optionalEntropy: null, DataProtectionScope.CurrentUser);
            return Task.FromResult<string?>(System.Text.Encoding.UTF8.GetString(unprotected));
        }
        catch
        {
            return Task.FromResult<string?>(null);
        }
    }

    public Task SetAsync(string token)
    {
        var data = System.Text.Encoding.UTF8.GetBytes(token);
        var protectedBytes = ProtectedData.Protect(data, optionalEntropy: null, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(FilePath, protectedBytes);
        return Task.CompletedTask;
    }
}
