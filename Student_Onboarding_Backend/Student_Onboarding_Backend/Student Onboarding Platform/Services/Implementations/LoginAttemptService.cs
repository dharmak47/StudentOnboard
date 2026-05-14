using Student_Onboarding_Platform.Data.Repositories.Interfaces;
using Student_Onboarding_Platform.Models.Entities;
using Student_Onboarding_Platform.Services.Interfaces;

namespace Student_Onboarding_Platform.Services.Implementations;

public class LoginAttemptService : ILoginAttemptService
{
    private readonly ILoginAttemptRepository _loginAttemptRepository;

    public LoginAttemptService(ILoginAttemptRepository loginAttemptRepository)
    {
        _loginAttemptRepository = loginAttemptRepository;
    }

    public async Task LogAttemptAsync(string email, string? ipAddress, string? userAgent, bool isSuccessful, string? failureReason)
    {
        var attempt = new LoginAttempt
        {
            Email = email,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            IsSuccessful = isSuccessful,
            FailureReason = failureReason
        };

        await _loginAttemptRepository.CreateAsync(attempt);
    }

    public async Task<int> GetRecentFailedAttemptsAsync(string email, TimeSpan window)
    {
        var since = DateTime.UtcNow.Subtract(window);
        return await _loginAttemptRepository.GetRecentFailedCountAsync(email, since);
    }
}
