using Microsoft.Extensions.DependencyInjection;

namespace PayHelp.Mobile.Maui.Helpers;

public static class ServiceHelper
{
    private static IServiceProvider? _provider;

    public static void Initialize(IServiceProvider provider)
        => _provider = provider;

    public static T? GetService<T>() where T : class
        => (_provider ?? throw new InvalidOperationException("ServiceHelper not initialized")).GetService<T>();
}
