using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace PayHelp.Mobile.Maui.Utilities;





public class StringNotNullOrEmptyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string s && !string.IsNullOrWhiteSpace(s);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
