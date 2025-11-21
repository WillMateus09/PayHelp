using Microsoft.Extensions.DependencyInjection;
using PayHelp.Mobile.Maui.ViewModels;

namespace PayHelp.Mobile.Maui.Services;

public static class ServicesConfig
{
    public const string BaseUrl = "http://192.168.15.107:5236/api/";

    public static void Configure(IServiceCollection services)
    {
        // HttpClient configurado
        services.AddHttpClient("api", client =>
        {
            client.BaseAddress = new Uri(BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler())
        .AddHttpMessageHandler<AuthorizedHttpMessageHandler>();

        // Handler precisa ser Transient
        services.AddTransient<AuthorizedHttpMessageHandler>();

        // Services
        services.AddSingleton<AuthService>();
        services.AddSingleton<ChamadoService>();
        services.AddSingleton<FaqService>();
        services.AddSingleton<MensagemService>();
        services.AddSingleton<RelatorioService>();
        services.AddSingleton<TriagemService>();

        // ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<CadastroViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ChamadosViewModel>();
        services.AddTransient<ChamadoDetalheViewModel>();
        services.AddTransient<AbrirChamadoViewModel>();
        services.AddTransient<MensagensViewModel>();
        services.AddTransient<FeedbacksViewModel>();
        services.AddTransient<FeedbackListaViewModel>();
        services.AddTransient<MarcarResolvidoViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<AdminUsersViewModel>();
        services.AddTransient<AdminSettingsViewModel>();
    }
}
