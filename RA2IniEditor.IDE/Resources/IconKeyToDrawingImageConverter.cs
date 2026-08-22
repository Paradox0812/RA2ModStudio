using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace RA2IniEditor.IDE.Resources;

/// <summary>
/// Resolves Project Explorer and AI action icon resource keys to ImageSource resources.
/// </summary>
public sealed class IconKeyToDrawingImageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string key = value as string ?? string.Empty;
        if (Application.Current is not null && Application.Current.TryFindResource(key) is ImageSource image)
            return image;

        if (Application.Current is not null && Application.Current.TryFindResource("Icon.Section") is ImageSource fallback)
            return fallback;

        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("Icon resources are one-way view resources.");
    }
}
