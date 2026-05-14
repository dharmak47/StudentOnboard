using Student_Onboarding_Platform.Models.Entities;

namespace Student_Onboarding_Platform.Data.Repositories.Interfaces;

public interface IOtpRepository
{
    Task<OtpVerification> CreateAsync(OtpVerification otp);
    Task<OtpVerification?> GetLatestValidAsync(string email, string otpType);
    Task IncrementAttemptCountAsync(Guid otpId);
    Task MarkAsUsedAsync(Guid otpId);
    Task InvalidateByEmailAndTypeAsync(string email, string otpType);
}
