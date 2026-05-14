namespace Student_Onboarding_Platform.Models.DTOs.Auth;

public class VerifyOtpRequest
{
    public string Email { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
    public string OtpType { get; set; } = string.Empty;
}
