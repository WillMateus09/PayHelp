using System.Text;
using System.Text.Json;

namespace PayHelp.Client;

public static class JwtHelper
{
    public static async Task<Guid?> TryGetUserIdAsync(ITokenStore tokenStore)
    {
        var token = await tokenStore.GetAsync().ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(token)) return null;
        try
        {
            // JWT format: header.payload.signature
            var parts = token.Split('.');
            if (parts.Length < 2) return null;
            var payload = parts[1].PadRight(parts[1].Length + (4 - parts[1].Length % 4) % 4, '=');
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload.Replace('-', '+').Replace('_', '/')));
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            string?[] keys = new[] { "nameid", "nameidentifier", "sub", "uid", "UserId", "userid" };
            foreach (var k in keys)
            {
                if (root.TryGetProperty(k!, out var el))
                {
                    var s = el.GetString();
                    if (Guid.TryParse(s, out var g)) return g;
                }
            }
            return null;
        }
        catch
        {
            return null;
        }
    }
}
