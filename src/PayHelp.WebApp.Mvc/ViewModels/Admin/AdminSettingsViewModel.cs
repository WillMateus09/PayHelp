namespace PayHelp.WebApp.Mvc.ViewModels;

public class AdminSettingsViewModel
{
    public string SupportVerificationWord { get; set; } = string.Empty;
    public string PublicBaseUrl { get; set; } = string.Empty; // Ex.: http://192.168.15.109:5236
}
