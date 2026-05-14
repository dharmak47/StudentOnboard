using System.Globalization;

namespace StudentOnboardingApp.Converters;

public class NotificationIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "StudentApproved" => "\u2705",     // green check
            "StudentDenied" => "\u274C",        // red X
            "CourseRegistration" => "\uD83D\uDCDA",  // books
            "PaymentUpdate" => "\uD83D\uDCB3",      // credit card
            "NewRegistration" => "\uD83D\uDC64",    // person
            "AdminMessage" => "\uD83D\uDCE2",         // loudspeaker
            _ => "\uD83D\uDD14"                      // bell
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
