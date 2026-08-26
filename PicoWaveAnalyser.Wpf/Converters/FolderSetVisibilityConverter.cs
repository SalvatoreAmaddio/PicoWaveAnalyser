using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PicoWaveAnalyser.Wpf.Converters;

[ValueConversion(typeof(bool), typeof(Visibility))]
public sealed class FolderSetVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not bool isVisible)
            throw new InvalidOperationException("The value must be a boolean");

        if (!isVisible) 
        {
            return Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}