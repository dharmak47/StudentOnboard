using StudentOnboardingApp.Models.Common;
using StudentOnboardingApp.Models.Onboarding;

namespace StudentOnboardingApp.Services.Interfaces;

public interface IOnboardingService
{
    Task<ApiResponse<ApprovalStatusResponse>> GetApprovalStatusAsync(string email);
    Task<ApiResponse<List<OnboardingInstructionDto>>> GetInstructionsAsync();
    Task<ApiResponse<string>> AcceptOnboardingAsync();
}
