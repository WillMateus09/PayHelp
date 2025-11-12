namespace PayHelp.Desktop.WinForms;

public class SessionContext
{
    public ApiClient.AuthResponse? CurrentUser { get; set; }
    public string? AccessToken { get; set; }
    public bool IsAuthenticated => CurrentUser != null;
    public bool IsSupport => string.Equals(CurrentUser?.Role, "Suporte", StringComparison.OrdinalIgnoreCase);
}
