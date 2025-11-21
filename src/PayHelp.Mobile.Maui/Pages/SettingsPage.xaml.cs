using PayHelp.Mobile.Maui.ViewModels;

namespace PayHelp.Mobile.Maui.Pages;

public partial class SettingsPage : ContentPage
{
	public SettingsPage()
	{
		InitializeComponent();
		BindingContext = new SettingsViewModel();
	}
}