using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using PayHelp.Mobile.Maui.Helpers;

namespace PayHelp.Mobile.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        Services.ServicesConfig.Configure(builder.Services);

        var app = builder.Build();

        ServiceHelper.Initialize(app.Services);
        return app;
    }
}
