using Student_Onboarding_Platform.Models.Entities;

namespace Student_Onboarding_Platform.Data.Repositories.Interfaces;

public interface ILoginAttemptRepository
{
    Task CreateAsync(LoginAttempt attempt);
    Task<int> GetRecentFailedCountAsync(string email, DateTime since);
}
