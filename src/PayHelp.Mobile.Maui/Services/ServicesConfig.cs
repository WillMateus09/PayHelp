using Microsoft.Extensions.DependencyInjection;
using PayHelp.Mobile.Maui.Utilities;
using PayHelp.Mobile.Maui.ViewModels;

namespace PayHelp.Mobile.Maui.Services;

public static class ServicesConfig
{
    public static void Configure(IServiceCollection services)
    {
        services.AddTransient<AuthorizedHttpMessageHandler>();

        services.AddHttpClient("api", client =>
        {
            var baseUrl = AppSettings.BaseApiUrl;
            if (!baseUrl.EndsWith("/")) baseUrl += "/";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
#if ANDROID
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            // Explicitly allow insecure (HTTP) for LAN dev; no custom cert validation
            ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true
        })
#endif
        .AddHttpMessageHandler<AuthorizedHttpMessageHandler>();


        services.AddSingleton<AuthService>();
        services.AddSingleton<ChamadoService>();
        services.AddSingleton<MensagemService>();
        services.AddSingleton<RelatorioService>();
        services.AddSingleton<TriagemService>();
    services.AddSingleton<FaqService>();


        services.AddTransient<LoginViewModel>();
        services.AddTransient<CadastroViewModel>();
        services.AddTransient<ChamadosViewModel>();
        services.AddTransient<AbrirChamadoViewModel>();
    services.AddTransient<ChamadoDetalheViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<MensagensViewModel>();
    }
}
