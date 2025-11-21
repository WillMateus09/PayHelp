using CommunityToolkit.Mvvm.ComponentModel;

namespace PayHelp.Mobile.Maui.Models;

public partial class AdminUserDto : ObservableObject
{
    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private string _nome = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _role = string.Empty;

    [ObservableProperty]
    private bool _isBlocked;
}
