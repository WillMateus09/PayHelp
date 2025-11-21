using System.Globalization;

namespace PayHelp.Mobile.Maui.Utilities;

/// <summary>
/// Conversor que retorna true se o status NÃO for "Encerrado"
/// </summary>
public class NotEncerradoConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            return !string.Equals(status, "Encerrado", StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
