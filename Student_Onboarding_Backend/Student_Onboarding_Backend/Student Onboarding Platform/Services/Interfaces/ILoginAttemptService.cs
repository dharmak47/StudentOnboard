namespace Student_Onboarding_Platform.Services.Interfaces;

public interface ILoginAttemptService
{
    Task LogAttemptAsync(string email, string? ipAddress, string? userAgent, bool isSuccessful, string? failureReason);
    Task<int> GetRecentFailedAttemptsAsync(string email, TimeSpan window);
}
