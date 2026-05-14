using System.Globalization;

namespace StudentOnboardingApp.Converters;

public class DateTimeFormatConverter : IValueConverter
{
    public string Format { get; set; } = "MMM dd, yyyy";

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime dt)
            return dt.ToString(Format);
        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
