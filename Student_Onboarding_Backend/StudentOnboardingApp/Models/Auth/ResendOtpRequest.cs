namespace StudentOnboardingApp.Models.Auth;

public class ResendOtpRequest
{
    public string Email { get; set; } = string.Empty;
    public string OtpType { get; set; } = string.Empty;
}
