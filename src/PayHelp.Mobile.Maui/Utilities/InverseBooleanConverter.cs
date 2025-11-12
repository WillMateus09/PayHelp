using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace PayHelp.Mobile.Maui.Utilities;




public class InverseBooleanConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : value;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : value;
}
