namespace Student_Onboarding_Platform.Services.Interfaces;

public interface IOtpService
{
    Task<string> GenerateAndStoreOtpAsync(Guid? userId, string email, string otpType);
    Task<bool> ValidateOtpAsync(string email, string otpCode, string otpType);
}
