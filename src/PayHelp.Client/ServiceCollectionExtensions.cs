using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace PayHelp.Client;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPayHelpClient(this IServiceCollection services, IConfiguration cfg)
    {
        // Read from config and environment variables (PAYHELP_API_BASE or PAYHELP_API_URL)
        var configuredBase = cfg["Api:BaseUrl"]; // appsettings
        var envOverride = Environment.GetEnvironmentVariable("PAYHELP_API_BASE")
            ?? Environment.GetEnvironmentVariable("PAYHELP_API_URL");

        // Fallback defensivo para evitar exceções na inicialização quando nada foi configurado.
        var candidate = envOverride ?? configuredBase;
        if (string.IsNullOrWhiteSpace(candidate))
        {
            candidate = "http://localhost:5236/api/"; // valor padrão de desenvolvimento
            System.Diagnostics.Debug.WriteLine("[PayHelp.Client] BaseUrl ausente – usando fallback http://localhost:5236/api/");
        }
        var normalized = ApiBaseUrlHelper.NormalizeBaseUrl(candidate);

        services.Configure<ApiOptions>(opt =>
        {
            opt.BaseUrl = normalized;
        });

        // HTTP client setup with no auto-redirect and token handler
        services.AddTransient<AuthenticatedHttpMessageHandler>();
        services.AddHttpClient<ApiService>((sp, http) =>
        {
            try
            {
                var opts = sp.GetRequiredService<IOptions<ApiOptions>>().Value;
                var baseUrl = ApiBaseUrlHelper.NormalizeBaseUrl(opts?.BaseUrl);
                http.BaseAddress = new Uri(baseUrl);
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AddPayHelpClient] Erro ao configurar HttpClient: {ex.Message}");
                // Fallback: usa localhost
                http.BaseAddress = new Uri("http://localhost:5236/api/");
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            AllowAutoRedirect = false
        })
        .AddHttpMessageHandler<AuthenticatedHttpMessageHandler>();

        // SignalR chat client
        services.AddSingleton<ChatClientService>();

        // NOTE: ITokenStore must be registered by the host app (MAUI/WinForms)

        return services;
    }
}
