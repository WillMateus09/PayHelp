using Microsoft.Maui.Controls;

namespace PayHelp.Mobile.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        UserAppTheme = AppTheme.Light;


        MainPage = new NavigationPage(new Views.LoginPage());


        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            System.Diagnostics.Debug.WriteLine($"[UnhandledException] {e.ExceptionObject}");
        };
        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            System.Diagnostics.Debug.WriteLine($"[UnobservedTaskException] {e.Exception}");
            e.SetObserved();
        };
    }
}
