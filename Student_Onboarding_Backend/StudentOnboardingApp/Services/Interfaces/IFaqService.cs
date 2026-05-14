using StudentOnboardingApp.Models.Common;
using StudentOnboardingApp.Models.Faq;

namespace StudentOnboardingApp.Services.Interfaces;

public interface IFaqService
{
    Task<ApiResponse<List<FaqDto>>> GetFaqsAsync();
}
