namespace Student_Onboarding_Platform.Models.DTOs.Auth;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? DeviceType { get; set; }
    public string? DeviceName { get; set; }
}
