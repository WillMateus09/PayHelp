using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace PayHelp.Mobile.Maui.Utilities
{
    /// <summary>
    /// Conversor para colorir as estrelas baseado na nota selecionada
    /// </summary>
    public class NotaToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int notaSelecionada && parameter is string paramStr && int.TryParse(paramStr, out int posicaoEstrela))
            {
                return posicaoEstrela <= notaSelecionada ? Colors.Gold : Colors.Gray;
            }
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Conversor para mudar o texto da estrela baseado na nota
    /// </summary>
    public class NotaToStarTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int notaSelecionada && parameter is string paramStr && int.TryParse(paramStr, out int posicaoEstrela))
            {
                return posicaoEstrela <= notaSelecionada ? "★" : "☆";
            }
            return "☆";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
