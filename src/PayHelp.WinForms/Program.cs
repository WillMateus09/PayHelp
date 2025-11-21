using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PayHelp.Client;

namespace PayHelp.WinForms;

internal static class Program
{
    public static IHost? HostInstance { get; private set; }
    public static IServiceProvider Services => HostInstance!.Services;

    [STAThread]
    static void Main()
    {
        // Cultura padrão pt-BR
        var culture = new CultureInfo("pt-BR");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        ApplicationConfiguration.Initialize();

        HostInstance = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(cfg =>
            {
                cfg.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                   .AddEnvironmentVariables();
            })
            .ConfigureServices((ctx, services) =>
            {
                services.AddSingleton<ITokenStore, WinFormsTokenStore>();
                services.AddPayHelpClient(ctx.Configuration);

                // Forms
                services.AddTransient<LoginForm>();
                services.AddTransient<MainForm>();
                services.AddTransient<ChatForm>();
                services.AddTransient<AboutForm>();
            })
            .Build();

        using (HostInstance)
        {
            var login = Services.GetRequiredService<LoginForm>();
            Application.Run(login);
        }
    }
}
