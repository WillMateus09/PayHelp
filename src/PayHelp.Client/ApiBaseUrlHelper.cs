using System;

namespace PayHelp.Client;

public static class ApiBaseUrlHelper
{
    public static string NormalizeBaseUrl(string? baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            // Fallback para desenvolvimento local
            System.Diagnostics.Debug.WriteLine("[ApiBaseUrlHelper] BaseUrl null/empty – usando fallback localhost:5236");
            return "http://localhost:5236/api/";
        }

        // Ensure trailing slash
        if (!baseUrl.EndsWith("/", StringComparison.Ordinal))
            baseUrl += "/";

        return baseUrl;
    }

    public static string ExtractRoot(string apiBaseUrl)
    {
        // Remove "/api" or "/api/" suffix if present
        var url = apiBaseUrl.TrimEnd('/');
        if (url.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
        {
            url = url[..^4]; // remove '/api'
        }
        // Ensure trailing slash after root
        if (!url.EndsWith("/", StringComparison.Ordinal))
            url += "/";
        return url.TrimEnd('/');
    }
}
