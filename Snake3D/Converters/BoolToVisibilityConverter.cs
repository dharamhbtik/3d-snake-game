using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Snake3D.Converters;

public sealed class BoolToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool boolVal = value is bool b && b;
        if (Invert) boolVal = !boolVal;
        return boolVal ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw foreign_not_supported();
        static NotImplementedException foreign_not_supported() => new();
    }
}
