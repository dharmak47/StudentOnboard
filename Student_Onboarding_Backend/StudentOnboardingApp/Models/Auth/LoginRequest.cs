namespace StudentOnboardingApp.Models.Auth;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DeviceType { get; set; } = "Mobile";
    public string DeviceName { get; set; } = DeviceInfo.Current.Name;
}
