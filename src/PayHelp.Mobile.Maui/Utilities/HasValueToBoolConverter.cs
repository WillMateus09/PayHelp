using System.Globalization;

namespace PayHelp.Mobile.Maui.Utilities;

public class HasValueToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime?)
        {
            return ((DateTime?)value).HasValue;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
