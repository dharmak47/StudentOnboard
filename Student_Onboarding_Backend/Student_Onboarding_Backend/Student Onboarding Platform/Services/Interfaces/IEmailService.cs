namespace Student_Onboarding_Platform.Services.Interfaces;

public interface IEmailService
{
    Task SendOtpEmailAsync(string toEmail, string otpCode, string purpose);
    Task SendPasswordResetEmailAsync(string toEmail, string otpCode);
    Task SendWelcomeEmailAsync(string toEmail, string firstName);
    Task SendPendingApprovalEmailAsync(string toEmail, string firstName);
    Task SendApprovalEmailAsync(string toEmail, string firstName);
    Task SendDenialEmailAsync(string toEmail, string firstName, string? reason);
    Task SendCourseRegistrationEmailAsync(string toEmail, string firstName, string courseName);
}
